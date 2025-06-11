using RagdollEngine;
using UnityEngine;

namespace RagdollEngine
{

    public class LevelChangePlayerBehaviour : PlayerBehaviour
    {
        public override bool Evaluate()
        {
            foreach (Volume vol in volumes)
            {
                LevelChangeTrigger levelChange = vol.GetComponent<LevelChangeTrigger>();
                if (levelChange != null)
                {
                    ChangeLevel(levelChange); // Call the ChangeLevel method with the found levelChange trigger
                    //
                    return true; // Return true if a level change volume is found
                }
            }
            return false;
        }

        private void ChangeLevel(LevelChangeTrigger levelChange)
        {
            //First we run save methods of relevant scripts
            //for (int i = 0; i < playerBehaviourTree.behaviours.Length; i++)
            //{
            //    //Check if the behaviour has a save method
            //    //if (playerBehaviourTree.behaviours[i] is LollipopCollectionPlayerBehaviour)
            //    //{
            //    //    //Call the save method
            //    //    //((LollipopCollectionPlayerBehaviour)playerBehaviourTree.behaviours[i]).SaveLollipops();
            //    //}
            //}

            //Then we run the level change method

            levelChange.ChangeLevel();

        }
    }


}