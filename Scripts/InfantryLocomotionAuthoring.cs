using System;
using System.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

using ProjectDawn.Navigation.Hybrid;

[RequireComponent(typeof(AgentAuthoring))]
[DisallowMultipleComponent]
public class InfantryLocomotionAuthoring : MonoBehaviour
{
    [SerializeField]
    float Speed = 3.5f;

    [SerializeField]
    float Acceleration = 8;

    [SerializeField]
    float AngularSpeed = 120;

    [SerializeField]
    float StoppingDistance = 0;

    [SerializeField]
    bool AutoBreaking = true;

    [SerializeField]
    float StopTurnRadius = 10;

    [SerializeField]
    float SlowDownDistance = 5f;

    Entity m_Entity;

    /// <summary>
    /// Returns default component of <see cref="TankLocomotion"/>.
    /// </summary>
    public InfantryLocomotion DefaultLocomotion => new()
    {
        Speed = Speed,
        Acceleration = Acceleration,
        AngularSpeed = math.radians(AngularSpeed),
        StoppingDistance = StoppingDistance,
        AutoBreaking = AutoBreaking
    };
    public InfantryLocomotion EntityLocomotion
    {
        get => World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<InfantryLocomotion>(m_Entity);
        set => World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(m_Entity, value);
    }

    void Awake()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        m_Entity = GetComponent<AgentAuthoring>().GetOrCreateEntity();
        world.EntityManager.AddComponentData(m_Entity, DefaultLocomotion);
    }

    void OnDestroy()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world != null)
            world.EntityManager.RemoveComponent<InfantryLocomotion>(m_Entity);
    }
}

internal class InfantryLocomotionBaker : Baker<InfantryLocomotionAuthoring>
{
    public override void Bake(InfantryLocomotionAuthoring authoring)
    {
        AddComponent(GetEntity(TransformUsageFlags.Dynamic), authoring.DefaultLocomotion);
    }
}