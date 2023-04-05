using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

class ModelEditor
{
    static string modelAssetPath = "Assets/Allassets/Models";
    static string prefabGenPath = "Assets/Builds/Models";
    [MenuItem("Tools/Model/修改模型资源参数并生成模型预制体挂上动画机")]
    public static void GeneratorModelPrefab()
    {
        //获取文件夹下所有文件
        string[] paths = Directory.GetDirectories(modelAssetPath);
        int progress = 0;
        try
        {
            foreach (string path in paths)
            {
                Debug.Log("GeneratorModelPrefab 正在处理" + path);
                progress++;
                EditorUtility.DisplayProgressBar("正在生成预制体...", "请耐心等待", progress / paths.Length);
                string fileName = Path.GetFileNameWithoutExtension(path);
                SetModelSetup(fileName);
                GenerateAnimator(fileName);
                GeneratePrefab(fileName);
            }
        }
        catch
        {
            Debug.LogError("GeneratorModelPrefab ERROR!");
        }
        finally
        {
            Debug.Log("处理完毕");
            EditorUtility.ClearProgressBar();
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 修改模型参数设置
    /// </summary>
    /// <param name="fileName"></param>
    public static void SetModelSetup(string fileName)
    {
        string fbxPath = modelAssetPath + "/" + fileName + "/" + fileName + ".fbx";
        ModelImporter importer = (ModelImporter)AssetImporter.GetAtPath(fbxPath);
        if (importer && importer.globalScale != 100)
        {
            importer.globalScale = 100;
            importer.SaveAndReimport();
        }
    }

    /// <summary>
    /// 生成动画机
    /// </summary>
    /// <param name="fileName"></param>
    public static void GenerateAnimator(string fileName)
    {
        string path = modelAssetPath + "/" + fileName;
        if (!Directory.Exists(path))
        {
            Debug.LogError("GenerateAnimator 没有 " + fileName);
            return;
        }

        string animatorPath = path + "/" + fileName + ".controller";
        Debug.Log("-------GenerateAnimator " + animatorPath);
        if (File.Exists(animatorPath))
        {
            File.Delete(animatorPath);
        }

        //添加动画机参数
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(animatorPath);
        AnimatorControllerParameter ctParam = new AnimatorControllerParameter();
        ctParam.name = "speed";
        ctParam.type = AnimatorControllerParameterType.Float;
        ctParam.defaultFloat = 1.0f;
        controller.AddParameter(ctParam);

        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

        string fbxPath = path + "/" + fileName + ".fbx";
        UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        Vector3 offset = new Vector3(210, 100, 0);
        int index = 0;
        int count = 5;
        int row, col;
        foreach (UnityEngine.Object obj in objs)
        {
            if (obj.GetType() == typeof(AnimationClip))//找到动画片段
            {
                int pos = obj.name.LastIndexOf("m_a");
                string animName = obj.name.Substring(pos + 4);
                Debug.Log("GenerateAnimator " + animName);

                row = (int)(index / count);
                col = index % count;

                AnimatorState state = rootStateMachine.AddState(animName, new Vector3(col * offset.x, (row + 2) * offset.y, 0));
                state.motion = obj as AnimationClip;
                state.speedParameter = "speed";
                state.speedParameterActive = true;
                state.tag = animName;


                if (animName == "idle")
                {
                    rootStateMachine.defaultState = state;
                }

                index++;
            }
        }
    }

    /// <summary>
    /// 生成预制体
    /// </summary>
    /// <param name="fileName"></param>
    public static void GeneratePrefab(string fileName)
    {
        GameObject prefabObj = null;
        try
        {
            prefabObj = new GameObject();
            string fbxPath = modelAssetPath + "/" + fileName + "/" + fileName + ".fbx";
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            GameObject modelObj = (GameObject)PrefabUtility.InstantiatePrefab(obj);
            modelObj.name = "model";
            modelObj.transform.parent = prefabObj.transform;
            modelObj.transform.SetAsFirstSibling();
            prefabObj.name = fileName;

            //检查动画机
            string animatorPath = modelAssetPath + "/" + fileName + "/" + fileName + ".controller";
            AnimatorController ac = AssetDatabase.LoadAssetAtPath<AnimatorController>(animatorPath);
            if (ac == null)
            {
                Debug.LogError("模型 " + fileName + " 没有动画机");
                return;
            }
            var animator = modelObj.GetComponent<Animator>();
            if (animator == null)
            {
                animator = modelObj.AddComponent<Animator>();
            }
            animator.runtimeAnimatorController = ac;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            //存为预制体
            if (!Directory.Exists(prefabGenPath))
            {
                Directory.CreateDirectory(prefabGenPath);
            }
            string prefabPath = prefabGenPath + "/" + fileName + ".prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(prefabObj, prefabPath, InteractionMode.AutomatedAction);
        }
        catch
        {
            Debug.LogError("GeneratePrefab ERROR!");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(prefabObj);
        }
    }
}