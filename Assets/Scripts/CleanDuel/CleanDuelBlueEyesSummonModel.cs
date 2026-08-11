using System.Collections.Generic;
using UnityEngine;

namespace IcarusGame.CleanDuel
{
    /// <summary>
    /// Presentation-only, original low-poly white dragon used by the Blue-Eyes summon cut-in.
    /// It renders in an isolated off-screen scene area so it never owns or changes duel state.
    /// </summary>
    internal sealed class CleanDuelBlueEyesSummonModel
    {
        private readonly List<Object> ownedAssets = new List<Object>();
        private GameObject previewRoot;
        private Transform modelRoot;
        private Transform leftWing;
        private Transform rightWing;
        private Transform head;
        private Camera previewCamera;
        private Light keyLight;
        private Light rimLight;
        private RenderTexture renderTexture;

        public Texture Texture => renderTexture;

        public void Build(Transform parent)
        {
            if (previewRoot != null)
            {
                return;
            }

            previewRoot = new GameObject("Blue-Eyes 3D Preview Stage");
            previewRoot.transform.SetParent(parent, false);
            previewRoot.transform.localPosition = new Vector3(0f, -1000f, 0f);

            renderTexture = new RenderTexture(960, 640, 24, RenderTextureFormat.ARGB32)
            {
                name = "Blue-Eyes Summon Model Texture",
                antiAliasing = 4,
                filterMode = FilterMode.Bilinear,
                useMipMap = false
            };
            ownedAssets.Add(renderTexture);

            GameObject cameraObject = new GameObject("Blue-Eyes Preview Camera", typeof(Camera));
            cameraObject.transform.SetParent(previewRoot.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 0.55f, -8.4f);
            previewCamera = cameraObject.GetComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = Color.clear;
            previewCamera.fieldOfView = 34f;
            previewCamera.nearClipPlane = 0.1f;
            previewCamera.farClipPlane = 35f;
            previewCamera.allowHDR = true;
            previewCamera.allowMSAA = true;
            previewCamera.targetTexture = renderTexture;
            LookAtLocal(previewCamera.transform, new Vector3(0f, 0.25f, 0f));

            Material armor = CreateMaterial(
                "Pearl Dragon Armor",
                new Color(0.72f, 0.83f, 0.91f, 1f),
                new Color(0.08f, 0.38f, 0.72f, 1f));
            Material membrane = CreateMaterial(
                "Icy Wing Membrane",
                new Color(0.28f, 0.58f, 0.76f, 1f),
                new Color(0.03f, 0.25f, 0.58f, 1f));
            Material claws = CreateMaterial(
                "Dragon Claws",
                new Color(0.12f, 0.19f, 0.28f, 1f),
                new Color(0.01f, 0.05f, 0.12f, 1f));
            Material glow = CreateMaterial(
                "Azure Dragon Glow",
                new Color(0.2f, 0.9f, 1f, 1f),
                new Color(0.15f, 1.4f, 2.5f, 1f));

            GameObject modelObject = new GameObject("Original White Dragon Model");
            modelObject.transform.SetParent(previewRoot.transform, false);
            modelRoot = modelObject.transform;
            BuildDragon(armor, membrane, claws, glow);

            keyLight = CreateLight(
                "Pearl Key Light",
                LightType.Directional,
                new Color(0.78f, 0.9f, 1f),
                2.4f,
                new Vector3(34f, -28f, 0f));
            rimLight = CreateLight(
                "Azure Rim Light",
                LightType.Point,
                new Color(0.05f, 0.68f, 1f),
                8f,
                Vector3.zero);
            rimLight.range = 10f;

            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            if (previewRoot != null)
            {
                previewRoot.SetActive(visible);
            }
        }

        public void Animate(float normalized)
        {
            if (modelRoot == null)
            {
                return;
            }

            float reveal = Smooth01(Mathf.InverseLerp(0f, 0.34f, normalized));
            float settle = Smooth01(Mathf.InverseLerp(0.24f, 0.62f, normalized));
            float wingBeat = Mathf.Sin(normalized * Mathf.PI * 7f);
            float hover = Mathf.Sin(normalized * Mathf.PI * 4f) * 0.11f;

            modelRoot.localPosition = new Vector3(0f, Mathf.Lerp(-1.2f, -0.05f, reveal) + hover, Mathf.Lerp(1.6f, 0f, settle));
            modelRoot.localScale = Vector3.one * Mathf.Lerp(0.24f, 1f, reveal);
            modelRoot.localEulerAngles = new Vector3(
                Mathf.Lerp(18f, 4f, settle),
                Mathf.Lerp(-38f, 16f, reveal) + Mathf.Sin(normalized * Mathf.PI * 2f) * 4f,
                Mathf.Sin(normalized * Mathf.PI * 3f) * 2.5f);

            leftWing.localEulerAngles = new Vector3(4f, -12f - wingBeat * 18f, 8f + wingBeat * 12f);
            rightWing.localEulerAngles = new Vector3(4f, 12f + wingBeat * 18f, -8f - wingBeat * 12f);
            head.localEulerAngles = new Vector3(-4f + Mathf.Sin(normalized * Mathf.PI * 5f) * 3f, 0f, 0f);

            previewCamera.transform.localPosition = new Vector3(
                Mathf.Sin(normalized * Mathf.PI * 2f) * 0.28f,
                0.55f + hover * 0.2f,
                -8.4f);
            LookAtLocal(previewCamera.transform, new Vector3(0f, 0.25f, 0f));
            keyLight.intensity = 2.2f + reveal * 0.7f;
            rimLight.intensity = 6.5f + (0.5f + wingBeat * 0.5f) * 2.8f;
        }

        public void Dispose()
        {
            if (previewCamera != null)
            {
                previewCamera.targetTexture = null;
            }

            if (renderTexture != null && renderTexture.IsCreated())
            {
                renderTexture.Release();
            }

            if (previewRoot != null)
            {
                DestroyObject(previewRoot);
            }

            for (int i = 0; i < ownedAssets.Count; i++)
            {
                if (ownedAssets[i] != null)
                {
                    DestroyObject(ownedAssets[i]);
                }
            }

            ownedAssets.Clear();
            previewRoot = null;
            modelRoot = null;
            leftWing = null;
            rightWing = null;
            head = null;
            previewCamera = null;
            keyLight = null;
            rimLight = null;
            renderTexture = null;
        }

        private void BuildDragon(Material armor, Material membrane, Material claws, Material glow)
        {
            CreatePart("Armored Torso", PrimitiveType.Sphere, modelRoot, new Vector3(0f, 0f, 0.25f), new Vector3(0.84f, 0.7f, 1.52f), Vector3.zero, armor);
            CreatePart("Chest Plate", PrimitiveType.Sphere, modelRoot, new Vector3(0f, -0.05f, -0.75f), new Vector3(0.72f, 0.62f, 0.82f), Vector3.zero, armor);

            CreateBone("Lower Neck", modelRoot, new Vector3(0f, 0.18f, -0.78f), new Vector3(0f, 0.5f, -1.38f), 0.42f, armor);
            CreateBone("Upper Neck", modelRoot, new Vector3(0f, 0.5f, -1.35f), new Vector3(0f, 0.62f, -1.82f), 0.34f, armor);

            GameObject headObject = new GameObject("Dragon Head");
            headObject.transform.SetParent(modelRoot, false);
            headObject.transform.localPosition = new Vector3(0f, 0.62f, -1.85f);
            head = headObject.transform;
            CreatePart("Skull", PrimitiveType.Sphere, head, Vector3.zero, new Vector3(0.52f, 0.38f, 0.58f), Vector3.zero, armor);
            CreatePart("Long Muzzle", PrimitiveType.Sphere, head, new Vector3(0f, -0.08f, -0.47f), new Vector3(0.38f, 0.24f, 0.62f), Vector3.zero, armor);
            CreatePart("Lower Jaw", PrimitiveType.Cube, head, new Vector3(0f, -0.27f, -0.47f), new Vector3(0.5f, 0.12f, 0.58f), new Vector3(5f, 0f, 0f), claws);
            CreateEye("Left Azure Eye", new Vector3(-0.31f, 0.08f, -0.35f), glow);
            CreateEye("Right Azure Eye", new Vector3(0.31f, 0.08f, -0.35f), glow);
            CreateBone("Left Crown Horn", head, new Vector3(-0.28f, 0.25f, 0f), new Vector3(-0.48f, 0.7f, 0.24f), 0.075f, claws);
            CreateBone("Right Crown Horn", head, new Vector3(0.28f, 0.25f, 0f), new Vector3(0.48f, 0.7f, 0.24f), 0.075f, claws);
            CreateBone("Center Crown Horn", head, new Vector3(0f, 0.3f, 0.08f), new Vector3(0f, 0.78f, 0.36f), 0.065f, armor);

            leftWing = BuildWing("Left Wing", -1f, armor, membrane);
            rightWing = BuildWing("Right Wing", 1f, armor, membrane);

            BuildLeg("Left Foreleg", new Vector3(-0.58f, -0.18f, -0.62f), -1f, armor, claws);
            BuildLeg("Right Foreleg", new Vector3(0.58f, -0.18f, -0.62f), 1f, armor, claws);
            BuildLeg("Left Hindleg", new Vector3(-0.62f, -0.18f, 0.95f), -1f, armor, claws);
            BuildLeg("Right Hindleg", new Vector3(0.62f, -0.18f, 0.95f), 1f, armor, claws);

            Vector3 tailStart = new Vector3(0f, 0.05f, 1.5f);
            Vector3[] tailPoints =
            {
                new Vector3(0.12f, 0.08f, 2.2f),
                new Vector3(0.36f, 0.2f, 2.78f),
                new Vector3(0.65f, 0.38f, 3.22f),
                new Vector3(0.88f, 0.62f, 3.58f)
            };
            for (int i = 0; i < tailPoints.Length; i++)
            {
                CreateBone("Tail Segment " + (i + 1), modelRoot, tailStart, tailPoints[i], Mathf.Lerp(0.34f, 0.1f, i / 3f), armor);
                tailStart = tailPoints[i];
            }
        }

        private Transform BuildWing(string name, float side, Material armor, Material membrane)
        {
            GameObject pivotObject = new GameObject(name);
            pivotObject.transform.SetParent(modelRoot, false);
            pivotObject.transform.localPosition = new Vector3(side * 0.6f, 0.48f, -0.02f);
            Transform pivot = pivotObject.transform;

            Vector3 shoulder = Vector3.zero;
            Vector3 elbow = new Vector3(side * 1.35f, 0.82f, 0.22f);
            Vector3 tip = new Vector3(side * 2.65f, 1.1f, 0.48f);
            Vector3 rear = new Vector3(side * 2.1f, -0.68f, 1.24f);
            Vector3 innerRear = new Vector3(side * 0.68f, -0.42f, 1.12f);
            CreateBone(name + " Shoulder", pivot, shoulder, elbow, 0.13f, armor);
            CreateBone(name + " Leading Edge", pivot, elbow, tip, 0.095f, armor);
            CreateBone(name + " Rear Strut", pivot, elbow, rear, 0.07f, armor);

            Mesh wingMesh = new Mesh { name = name + " Procedural Mesh" };
            wingMesh.vertices = new[] { shoulder, elbow, tip, rear, innerRear };
            wingMesh.triangles = new[]
            {
                0, 1, 4, 1, 3, 4, 1, 2, 3,
                4, 1, 0, 4, 3, 1, 3, 2, 1
            };
            wingMesh.RecalculateNormals();
            wingMesh.RecalculateBounds();
            ownedAssets.Add(wingMesh);

            GameObject membraneObject = new GameObject(name + " Membrane", typeof(MeshFilter), typeof(MeshRenderer));
            membraneObject.transform.SetParent(pivot, false);
            membraneObject.GetComponent<MeshFilter>().sharedMesh = wingMesh;
            membraneObject.GetComponent<MeshRenderer>().sharedMaterial = membrane;
            return pivot;
        }

        private void BuildLeg(string name, Vector3 shoulder, float side, Material armor, Material claws)
        {
            Vector3 knee = shoulder + new Vector3(side * 0.25f, -0.54f, -0.02f);
            Vector3 foot = knee + new Vector3(side * 0.08f, -0.38f, -0.25f);
            CreateBone(name + " Upper", modelRoot, shoulder, knee, 0.19f, armor);
            CreateBone(name + " Lower", modelRoot, knee, foot, 0.14f, armor);
            CreatePart(name + " Foot", PrimitiveType.Sphere, modelRoot, foot, new Vector3(0.2f, 0.13f, 0.32f), Vector3.zero, claws);
            for (int i = -1; i <= 1; i++)
            {
                Vector3 clawStart = foot + new Vector3(i * 0.1f, -0.02f, -0.18f);
                CreateBone(name + " Claw " + (i + 2), modelRoot, clawStart, clawStart + new Vector3(0f, -0.04f, -0.28f), 0.035f, claws);
            }
        }

        private void CreateEye(string name, Vector3 position, Material material)
        {
            CreatePart(name, PrimitiveType.Sphere, head, position, new Vector3(0.08f, 0.065f, 0.035f), Vector3.zero, material);
        }

        private GameObject CreatePart(
            string name,
            PrimitiveType primitive,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Vector3 eulerAngles,
            Material material)
        {
            GameObject part = GameObject.CreatePrimitive(primitive);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.transform.localEulerAngles = eulerAngles;
            part.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyObject(collider);
            }

            return part;
        }

        private GameObject CreateBone(
            string name,
            Transform parent,
            Vector3 start,
            Vector3 end,
            float radius,
            Material material)
        {
            Vector3 direction = end - start;
            GameObject bone = CreatePart(
                name,
                PrimitiveType.Cylinder,
                parent,
                (start + end) * 0.5f,
                new Vector3(radius, direction.magnitude * 0.5f, radius),
                Vector3.zero,
                material);
            bone.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            return bone;
        }

        private Material CreateMaterial(string name, Color baseColor, Color emissionColor)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader) { name = name };
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", baseColor);
            }
            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", baseColor);
            }
            if (material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", emissionColor);
                material.EnableKeyword("_EMISSION");
            }
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.64f);
            }
            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0.18f);
            }

            ownedAssets.Add(material);
            return material;
        }

        private Light CreateLight(string name, LightType type, Color color, float intensity, Vector3 positionOrRotation)
        {
            GameObject lightObject = new GameObject(name, typeof(Light));
            lightObject.transform.SetParent(previewRoot.transform, false);
            Light light = lightObject.GetComponent<Light>();
            light.type = type;
            light.color = color;
            light.intensity = intensity;
            if (type == LightType.Directional)
            {
                lightObject.transform.localEulerAngles = positionOrRotation;
            }
            else
            {
                lightObject.transform.localPosition = positionOrRotation;
            }

            return light;
        }

        private static void LookAtLocal(Transform source, Vector3 localTarget)
        {
            source.localRotation = Quaternion.LookRotation(localTarget - source.localPosition, Vector3.up);
        }

        private static float Smooth01(float value)
        {
            value = Mathf.Clamp01(value);
            return value * value * (3f - 2f * value);
        }

        private static void DestroyObject(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(target);
            }
            else
            {
                Object.DestroyImmediate(target);
            }
        }
    }
}
