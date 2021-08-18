using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using MalbersAnimations.Scriptables;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace MalbersAnimations.Controller
{
    /// <summary>Use this Script's Transform as the Respawn Point</summary>
    [AddComponentMenu("Malbers/Animal Controller/Respawner")]
    public class MRespawner : MonoBehaviour
    {
        public static MRespawner instance;

        #region Respawn
        [Tooltip("Animal Prefab to Swpawn"), FormerlySerializedAs("playerPrefab")]
        public GameObject player;

        //[ContextMenuItem("Set Default", "SetDefaultRespawnPoint")]
        //public Vector3Reference RespawnPoint;
        public StateID RespawnState;
        public FloatReference RespawnTime = new FloatReference(4f);
        [Tooltip("If True: it will destroy the MainPlayer GameObject and Respawn a new One")]
        public BoolReference DestroyAfterRespawn = new BoolReference(true);


        /// <summary>Active Player Animal GameObject</summary>
        private GameObject InstantiatedPlayer;
        /// <summary>Active Player Animal</summary>
        private MAnimal activeAnimal;
        /// <summary>Old Player Animal GameObject</summary>
        private GameObject oldPlayer;

        #endregion

        public UnityEvent OnRestartGame = new UnityEvent();


        void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            OnRestartGame.Invoke();
            FindMainAnimal();
        }

        public virtual void SetPlayer(GameObject go) => player = go;

        void OnEnable()
        {
            if (!isActiveAndEnabled) return;

            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                gameObject.name = gameObject.name + " Instance";
                SceneManager.sceneLoaded += OnLevelFinishedLoading;
            }
            else
            {
                Destroy(gameObject); //Destroy This GO since is already a Spawner in the scene
            }

            FindMainAnimal();

            if (player == null)
            {
                Debug.LogWarning("[Respawner Removed]. There's no Character assigned");
                Destroy(gameObject); //Destroy This GO since is already a Spawner in the scene
            }
        }


        private void OnDisable()
        {
            if (instance == this)
            {
                SceneManager.sceneLoaded -= OnLevelFinishedLoading;
                if (activeAnimal != null)
                    activeAnimal.OnStateChange.RemoveListener(OnCharacterDead);  //Listen to the Animal changes of states
            }
        }

        public void ResetScene()
        {
            DestroyOldPlayer();

            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);

            Respawning = false;
        }

        /// <summary>Finds the Main Animal used as Player on the Active Scene</summary>
        void FindMainAnimal()
        {   
            activeAnimal = MAnimal.MainAnimal;

            if (player != null)
            {
                if (player.IsPrefab())
                {
                    InstantiateNewPlayer();
                }
                else
                {
                    activeAnimal = player.GetComponent<MAnimal>();
                }
                 

                if (activeAnimal)
                {
                    activeAnimal.OnStateChange.AddListener(OnCharacterDead);  //Listen to the Animal changes of states
                    activeAnimal.TeleportRot(transform);                         //Move the Animal to is Start Position
                    RespawnState = RespawnState ?? activeAnimal.OverrideStartState;
                    activeAnimal.OverrideStartState = RespawnState;
                }
            }
            else
            {
                activeAnimal = MAnimal.MainAnimal;

                if (activeAnimal != null) 
                {
                    player = activeAnimal.gameObject;
                    FindMainAnimal();
                }
            }
        }

        /// <summary>Listen to the Animal States</summary>
        public void OnCharacterDead(int StateID)
        {
            if (Respawning) return;

            if (StateID == StateEnum.Death)                      //Means Death
            {
                oldPlayer = InstantiatedPlayer;                  //Store the old player IMPORTANT
                Respawning = true;

                if (player != null && player.IsPrefab())         //If the Player is a Prefab then then instantiate it on the created scene
                {
                    StartCoroutine(C_SpawnPrefab());
                }
                else
                {
                    Invoke(nameof(ResetScene), RespawnTime);
                }
            }
        }

        private bool Respawning;

        public IEnumerator C_SpawnPrefab()
        {
            yield return new WaitForSeconds(RespawnTime);

            DestroyOldPlayer();

            yield return new WaitForEndOfFrame();

            InstantiateNewPlayer();
        }

        void DestroyOldPlayer()
        {
            if (oldPlayer != null)
            {
                if (DestroyAfterRespawn)
                    Destroy(oldPlayer);
                else
                    DestroyAllComponents(oldPlayer);
            }
        }

        void InstantiateNewPlayer()
        {
            InstantiatedPlayer = Instantiate(player, transform.position, transform.rotation);
            activeAnimal = InstantiatedPlayer.GetComponent<MAnimal>();
          
            activeAnimal.OverrideStartState = RespawnState;

            activeAnimal.OnStateChange.AddListener(OnCharacterDead);
            OnRestartGame.Invoke();

            Respawning = false;
        }


        /// <summary>Destroy all the components on  Animal and leaves the mesh and bones</summary>
        private void DestroyAllComponents(GameObject target)
        {
            if (!target) return;

            var components = target.GetComponentsInChildren<MonoBehaviour>();

            foreach (var comp in components) Destroy(comp);
        
            var colliders = target.GetComponentsInChildren<Collider>();

            if (colliders != null)
            {
                foreach (var col in colliders) Destroy(col);
            }

            var rb = target.GetComponentInChildren<Rigidbody>();
            if (rb != null) Destroy(rb);
            var anim = target.GetComponentInChildren<Animator>();
            if (anim != null) Destroy(anim);
        }
    }
}