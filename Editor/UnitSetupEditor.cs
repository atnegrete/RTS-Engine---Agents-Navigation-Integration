using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using Unity.VisualScripting;
using ProjectDawn.Navigation.Hybrid;
using RTSEngine.Movement;

namespace RTSEngineNavAgentsIntegration {

    [CustomEditor(typeof(UnitSetup))]
    public class UnitSetupEditor : Editor
    {

        protected UnitSetup Instance { get; set; }
        SerializedProperty ConfigCompleted;

        SerializedProperty UnitType;
        SerializedProperty AvoidanceType;

        SerializedProperty AgentsNavController;
        SerializedProperty Agent;
        SerializedProperty Shape;
        SerializedProperty NavMesh;
        SerializedProperty Avoid;
        SerializedProperty Separation;
        SerializedProperty SmartStop;

        private void OnEnable()
        {
            Instance = (UnitSetup)target;

            ConfigCompleted = serializedObject.FindProperty("ConfigCompleted");

            AgentsNavController = serializedObject.FindProperty("AgentsNavController");
            UnitType = serializedObject.FindProperty("UnitType");
            AvoidanceType = serializedObject.FindProperty("AvoidanceType");
            Agent = serializedObject.FindProperty("Agent");
            Shape = serializedObject.FindProperty("Shape");
            NavMesh = serializedObject.FindProperty("NavMesh");
            Avoid = serializedObject.FindProperty("Avoid");
            Separation = serializedObject.FindProperty("Separation");
            SmartStop = serializedObject.FindProperty("SmartStop");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            string[] mainRow = { "Configuration", "Debug" };
            Instance.TabID = GUILayout.SelectionGrid(Instance.TabID, mainRow, 2);

            switch (Instance.TabID)
            {
                case 0:
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.PropertyField(ConfigCompleted);
                    EditorGUILayout.PropertyField(UnitType);
                    EditorGUILayout.PropertyField(AvoidanceType);

                    if (Instance.UnitType != RTSEngineNavAgentsIntegration.UnitType.None)
                    {
                        if (GUILayout.Button("Configure Unit"))
                        {
                            bool useSonarAvoidance = Instance.AvoidanceType == RTSEngineNavAgentsIntegration.AvoidanceType.Sonar;

                            if (Instance.transform.TryGetComponent(out NavMeshAgent OldAgent))
                            {
                                DestroyImmediate(OldAgent);
                            }

                            if (Instance.transform.TryGetComponent(out NavMeshObstacle OldObstacle))
                            {
                                DestroyImmediate(OldObstacle);
                            }

                            // remove rtse nav controller (typically in unit_movement)
                            if (Instance.transform.TryGetComponent(out NavMeshAgentController OldRTSEAgentController))
                            {
                                DestroyImmediate(OldRTSEAgentController);
                            }

                            Instance.AgentsNavController = Instance.AddComponent<AgentsNavController>();
                            Instance.AgentsNavController.navAgent = Instance.AddComponent<AgentAuthoring>();
                            Instance.AgentsNavController.agentShape = Instance.AddComponent<AgentCylinderShapeAuthoring>();
                            Instance.AgentsNavController.agentNavmesh = Instance.AddComponent<AgentNavMeshAuthoring>();

                            if (useSonarAvoidance)
                            {
                                Instance.AgentsNavController.agentAvoidance = Instance.AddComponent<AgentAvoidAuthoring>();
                            }
                            else
                            {
                                Instance.AgentsNavController.agentSeparation = Instance.AddComponent<AgentSeparationAuthoring>();
                            }

                            Instance.AddComponent<AgentSmartStopAuthoring>();
                            switch (Instance.UnitType)
                            {
                                case RTSEngineNavAgentsIntegration.UnitType.Infantry:
                                    Instance.AddComponent<InfantryLocomotionAuthoring>();
                                    break;
                                case RTSEngineNavAgentsIntegration.UnitType.Wheeled:
                                    Instance.AddComponent<WheeledLocomotionAuthoring>();
                                    break;
                                case RTSEngineNavAgentsIntegration.UnitType.Tracked:
                                    Instance.AddComponent<TrackedLocomotionAuthoring>();
                                    break;
                                case RTSEngineNavAgentsIntegration.UnitType.Air:
                                    Instance.AddComponent<AircraftLocomotionAuthoring>();
                                    break;
                            }

                        }
                    }

                    EditorGUILayout.EndVertical();
                    break;
                case 1:
                    EditorGUILayout.BeginVertical("box");


                    EditorGUILayout.EndVertical();
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

    }

}
