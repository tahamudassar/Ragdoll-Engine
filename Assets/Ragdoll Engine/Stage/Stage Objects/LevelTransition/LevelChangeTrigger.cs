using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR


[CustomEditor(typeof(LevelChangeTrigger))]
public class LevelChangeVolumeEditor : Editor
{
    private string[] sceneNames; // Array to store scene names
    private int previousSelectedIndex; // Track previous selection

    private void OnEnable()
    {
        // Retrieve all scenes in the build settings
        var scenes = EditorBuildSettings.scenes;
        sceneNames = new string[scenes.Length];

        for (int i = 0; i < scenes.Length; i++)
        {
            sceneNames[i] = System.IO.Path.GetFileNameWithoutExtension(scenes[i].path); // Get scene name
        }

        // Get current selection
        LevelChangeTrigger levelChangeTrigger = (LevelChangeTrigger)target;
        previousSelectedIndex = levelChangeTrigger.SelectedSceneIndex;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        LevelChangeTrigger levelChangeTrigger = (LevelChangeTrigger)target;

        // Ensure the selected index is within bounds
        if (levelChangeTrigger.SelectedSceneIndex >= sceneNames.Length && sceneNames.Length > 0)
        {
            levelChangeTrigger.SelectedSceneIndex = 0;
        }

        EditorGUI.BeginChangeCheck();

        // Dropdown for selecting a scene
        int newSelectedIndex = EditorGUILayout.Popup("Select Level", levelChangeTrigger.SelectedSceneIndex, sceneNames);

        // Only update if the user changed the selection
        if (EditorGUI.EndChangeCheck())
        {
            // User made a selection change
            Undo.RecordObject(levelChangeTrigger, "Change Level Selection");

            levelChangeTrigger.SelectedSceneIndex = newSelectedIndex;

            // Only set the level name if we have scenes in the build settings and a valid selection
            if (sceneNames.Length > 0 && newSelectedIndex >= 0 && newSelectedIndex < sceneNames.Length)
            {
                levelChangeTrigger.SetLevel(sceneNames[newSelectedIndex]);
            }

            // Mark the object as dirty to ensure Unity saves the change
            EditorUtility.SetDirty(levelChangeTrigger);

            previousSelectedIndex = newSelectedIndex;
        }
    }
}
#endif
public class LevelChangeTrigger : Trigger
{
    [SerializeField] private string levelName; // Selected level name
    [SerializeField] private int selectedSceneIndex;

    public int SelectedSceneIndex
    {
        get => selectedSceneIndex;
        set => selectedSceneIndex = value;
    }
    public void SetLevel(string level)
    {
        levelName = level;
    }

    public void ChangeLevel()
    {
        if (!string.IsNullOrEmpty(levelName))
        {
            SceneManager.LoadScene(levelName);
        }
        else
        {
            Debug.LogWarning("Level name is not set!");
        }
    }
}


