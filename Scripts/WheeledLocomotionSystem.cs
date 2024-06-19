using Unity.Burst;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using ProjectDawn.Navigation;
using System;
using UnityEngine;

[BurstCompile]
[RequireMatchingQueriesForUpdate]
[UpdateInGroup(typeof(AgentLocomotionSystemGroup))]
public partial struct WheeledLocomotionSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new WheeledLocomotionJob
        {
            DeltaTime = state.WorldUnmanaged.Time.DeltaTime
        }.ScheduleParallel();
    }

    [BurstCompile]
    partial struct WheeledLocomotionJob : IJobEntity
    {
        public float DeltaTime;
        public float3 tempVelocity;
        public float speedMultiplier;

        public void Execute(ref LocalTransform transform, ref AgentBody body, in WheeledLocomotion locomotion, in AgentShape shape)
        {
            if (body.IsStopped)
                return;

            // Check, if we reached the destination
            //float slowDistance = locomotion.ReduceSpeedDistance;
            float remainingDistance = body.RemainingDistance;

            if (remainingDistance <= locomotion.StoppingDistance + 1e-3f)
            {
                body.Velocity = 0;
                body.IsStopped = true;
                return;
            }
            float maxSpeed = locomotion.Speed;


            // Start breaking if close to destination
            if (locomotion.AutoBreaking)
            {
                float breakDistance = shape.Radius * 4 + locomotion.StoppingDistance;
                if (remainingDistance <= breakDistance)
                {
                    maxSpeed = math.lerp(locomotion.Speed, 0, DeltaTime * locomotion.Acceleration);
                }
            }

            // Force force to be maximum of unit length, but can be less
            float forceLength = math.length(body.Force);
            if (forceLength > 1)
                body.Force = body.Force / forceLength;

            
            const float maxVelocityMagnitudeForSteering = 3f;
            body.Velocity = tempVelocity;
            speedMultiplier = 1;

            var velocityMagnitudeForSteering = math.clamp(math.length(body.Velocity), 0, maxVelocityMagnitudeForSteering);
            var magnitudeForSteering = velocityMagnitudeForSteering / maxVelocityMagnitudeForSteering;
            var forwardMultiplier = math.lerp(0, 1, magnitudeForSteering);

            var directionSign = 1;
            if (math.dot(transform.Forward(), math.normalize(body.Force)) < 0 && math.length(body.Velocity) <
            maxVelocityMagnitudeForSteering)
            {
                directionSign = 1;
            }
            var designatedForce = directionSign * math.lerp(transform.Forward() * math.length(body.Force), body.Force, forwardMultiplier);
            body.Velocity = math.lerp(body.Velocity, designatedForce * maxSpeed, math.saturate(DeltaTime * locomotion.Acceleration));

            float speed = math.length(body.Velocity);
            float angle = math.atan2(body.Velocity.x, body.Velocity.z);
            transform.Rotation = math.slerp(transform.Rotation, quaternion.RotateY(angle), DeltaTime * locomotion.AngularSpeed);

            // Early out if steps is going to be very small
            if (speed < 1e-3f)
                return;

            // Avoid over-stepping the destination
            if (speed * DeltaTime > remainingDistance)
            {
                transform.Position += (body.Velocity / speed) * remainingDistance;
                return;
            }

            // Update position
            transform.Position += DeltaTime * body.Velocity;
        }
    }
}
