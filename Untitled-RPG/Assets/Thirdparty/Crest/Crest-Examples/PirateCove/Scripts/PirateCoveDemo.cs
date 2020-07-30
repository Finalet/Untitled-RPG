// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using Crest;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public class PirateCoveDemo : MonoBehaviour
{
    [SerializeField] bool _demoEnabled = true;
    [SerializeField] DemoShot[] _shots = null;
    [SerializeField] float _fadeToBlackTime = 2f;
    [SerializeField] float _solidBlackTime = 0.2f;
    [SerializeField] float _initialBlackHold = 1f;
    [SerializeField] bool _triggerNextShot = false;
    [SerializeField] bool _showEscPrompt = false;
    [SerializeField] string _environmentScene = "PirateCove-Environment";

    [Header("UI")]
    [SerializeField] GameObject _uiParent = null;
    [SerializeField] Text _captionUI = null;
    [SerializeField] Image _fadeToBlackImage = null;
    [SerializeField] GameObject _bottomPanel = null;

    Animation _anim;
    int _shotIndex = -1;
    float _clipStartTime = 0f;
    float _clipEndTime = 0f;

    OceanDebugGUI _debugGUI;
    bool _showGUI = false;
    bool _showTargets = false;

    CamController camController;
    bool _firstOnGUI = true;

    private void Awake()
    {
        if (enabled)
        {
            _fadeToBlackImage.enabled = true;
            _fadeToBlackImage.color = Color.black;

            _bottomPanel.SetActive(true);

            _debugGUI = FindObjectOfType<OceanDebugGUI>();
            if (_debugGUI)
            {
                _showGUI = _debugGUI._guiVisible;
                _showTargets = _debugGUI._showOceanData;
                _debugGUI._guiVisible = false;
                _debugGUI._showOceanData = false;
            }

            camController = GetComponent<CamController>();
            if (camController)
            {
                camController.enabled = false;
            }
        }

        LoadEnvScene();
    }

    void LoadEnvScene()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.name == _environmentScene)
            {
                return;
            }
        }

        var loadedLevel = SceneManager.GetSceneByName(_environmentScene);
        if (!loadedLevel.isLoaded)
        {
#if UNITY_EDITOR
            var lsp = new LoadSceneParameters()
            {
                loadSceneMode = LoadSceneMode.Additive,
                localPhysicsMode = LocalPhysicsMode.Physics3D
            };
            EditorSceneManager.LoadSceneInPlayMode("Assets/Crest/Crest-Examples/PirateCove/Scenes/SubScenes/" + _environmentScene + ".unity", lsp);
#else
            SceneManager.LoadScene(_environmentScene, LoadSceneMode.Additive);
#endif
        }
    }

    private void Start()
    {
        _anim = GetComponent<Animation>();

        if (!_demoEnabled || _shots.Length == 0)
        {
            StopDemo();
            return;
        }

        DemoMusic.Instance?.Play();

        foreach (var shot in _shots)
        {
            if (shot._cameraAnimation != null)
            {
                _anim.AddClip(shot._cameraAnimation, shot._cameraAnimation.name);
            }
        }
    }

    private void Update()
    {
        bool skip = Input.GetKeyDown(KeyCode.N) || _triggerNextShot;
        if ((!_anim.isPlaying && Time.time > _initialBlackHold) || skip)
        {
            _triggerNextShot = false;

            _fadeToBlackImage.CrossFadeAlpha(1f, 0f, true);

            PlayNextShot();
        }

        if (_clipEndTime != 0f && ShotTimeRemaining < (_fadeToBlackTime + _solidBlackTime))
        {
            _fadeToBlackImage.CrossFadeAlpha(1f, ShotTimeRemaining - _solidBlackTime, false);
            _clipEndTime = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopDemo();
            return;
        }

        // Hide overlay UI
        if (Input.GetKeyDown(KeyCode.U))
        {
            _showEscPrompt = false;
            _bottomPanel.SetActive(false);
        }

        if (_shotIndex != -1)
        {
            _shots[_shotIndex].UpdateShot(ShotTime, ShotTimeRemaining);
        }
    }

    float ShotTime => Time.time - _clipStartTime;
    float ShotTimeRemaining => _clipEndTime - Time.time;

    private void PlayNextShot()
    {
        if (_shotIndex != -1)
        {
            _shots[_shotIndex].OnStop();
        }

        _shotIndex = (_shotIndex + 1) % _shots.Length;

        var shot = _shots[_shotIndex];

        if (shot._cameraAnimation != null)
        {
            _anim.Play(shot._cameraAnimation.name);
            _clipStartTime = Time.time;
            _clipEndTime = _clipStartTime + shot._cameraAnimation.length;
            _fadeToBlackImage.CrossFadeAlpha(0f, _fadeToBlackTime, false);
        }

        _captionUI.text = shot._demoText;

        shot.OnPlay();
    }

    private void StopDemo()
    {
        enabled = false;

        if (_anim)
        {
            _anim.Stop();
        }

        if (_shotIndex != -1)
        {
            _shots[_shotIndex].OnStop();
        }

        _captionUI.enabled = false;
        _fadeToBlackImage.CrossFadeAlpha(0f, _fadeToBlackTime, false);
        _uiParent.SetActive(false);

        if (camController) camController.enabled = true;

        if (_debugGUI)
        {
            _debugGUI._guiVisible = _showGUI;
            _debugGUI._showOceanData = _showTargets;
        }

        DemoMusic.Instance?.FadeOut();
    }

    private void OnGUI()
    {
        if (_firstOnGUI)
        {
            // Hacky way to force shaders to load early
            GUI.DrawTexture(new Rect(0, 0, 1, 1), Texture2D.blackTexture);
            GUI.DrawTexture(new Rect(0, 0, 1, 1), Texture2D.blackTexture, ScaleMode.ScaleAndCrop, false);
            GUI.Toggle(new Rect(0, 0, 30, 30), true, "Temp");
            _firstOnGUI = false;
        }

        if (_showEscPrompt)
        {
            GUI.Label(new Rect(10, 5, 250, 25), "[ESC]");
        }
    }
}
