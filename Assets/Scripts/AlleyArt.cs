using System.Collections.Generic;
using UnityEngine;

namespace TrashRac
{
    internal sealed partial class AlleyArt
    {
        private readonly Shader shader = Resources.Load<Shader>("AlleyToon");
        private readonly Dictionary<(string, float, float), Material> materials = new();
        private readonly Dictionary<(Vector3, int), Transform> cartonTemplates = new();
        private Transform templateRoot;

        public Transform Door { get; private set; }
        public Transform DoorLight { get; private set; }
        public Transform DoorSpill { get; private set; }
        public Transform Worker { get; private set; }
        public Transform WorkerArm { get; private set; }
        public Transform WorkerBox { get; private set; }
        public Transform Raccoon { get; private set; }
        public Transform RaccoonArm { get; private set; }
        public Transform Can { get; private set; }
        public Transform CanLid { get; private set; }

        public static Color Tint(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color color);
            return color;
        }

        public static Transform Group(string name, Vector3 position, Transform parent)
        {
            var transform = new GameObject(name).transform;
            transform.SetParent(parent, false);
            transform.localPosition = position;
            return transform;
        }

        public Transform Part(PrimitiveType kind, string name, Transform parent, Vector3 at,
            Vector3 scale, string color, float ink = 0.014f, float glow = 0, bool collider = false)
        {
            var part = GameObject.CreatePrimitive(kind);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = at;
            part.transform.localScale = scale;
            if (!collider && part.TryGetComponent(out Collider component))
            {
                component.enabled = false;
                Object.Destroy(component);
            }

            part.GetComponent<Renderer>().sharedMaterial = MaterialFor(color, ink, glow);
            return part.transform;
        }

        private Material MaterialFor(string color, float ink, float glow)
        {
            var key = (color, ink, glow);
            if (materials.TryGetValue(key, out Material material)) return material;
            material = new Material(shader);
            material.SetColor("_Color", Tint(color));
            material.SetFloat("_Outline", ink);
            material.SetFloat("_Glow", glow);
            material.SetShaderPassEnabled("INK", ink > 0);
            materials.Add(key, material);
            return material;
        }

        private Transform Cube(string name, Transform parent, Vector3 at, Vector3 scale,
            string color, float ink = 0.014f, float glow = 0)
        {
            return Part(PrimitiveType.Cube, name, parent, at, scale, color, ink, glow);
        }

        private Transform Ball(string name, Transform parent, Vector3 at, Vector3 scale, string color)
        {
            return Part(PrimitiveType.Sphere, name, parent, at, scale, color);
        }

        private void Shadow(Transform parent, float x, float z, float width, float depth)
        {
            Part(PrimitiveType.Cylinder, "Contact shadow", parent, new Vector3(x, 0.009f, z),
                new Vector3(width, 0.007f, depth), "211e31", 0, 0.65f);
        }

        public void Build(Transform root)
        {
            templateRoot = Group("Carton templates", Vector3.zero, root);
            templateRoot.gameObject.SetActive(false);
            Part(PrimitiveType.Cube, "Alley floor", root, new Vector3(0, -0.15f, 2),
                new Vector3(60, 0.3f, 38), "382a54", 0, 0.25f, true);

            var skyline = Group("Skyline", Vector3.zero, root);
            Part(PrimitiveType.Sphere, "Moon", skyline, new Vector3(8.4f, 4.65f, 14),
                Vector3.one * 1.35f, "ffd49a", 0.018f, 1);
            for (int i = 0; i < 32; i++)
            {
                float x = (i * 17 % 61) * 0.49f - 15;
                float y = 3.15f + (i * 13 % 29) * 0.072f;
                Part(PrimitiveType.Sphere, "Star", skyline, new Vector3(x, y, 17),
                    Vector3.one * (i % 5 == 0 ? 0.055f : 0.027f), "d8bcb5", 0, 1);
            }

            for (int i = 0; i < 13; i++)
            {
                float x = -15 + i * 2.35f;
                float h = 3 + (i * 7 % 6) * 0.42f;
                Cube("Building", skyline, new Vector3(x, h / 2, 12), new Vector3(2.15f, h, 2),
                    i % 2 == 0 ? "2e284e" : "36305b", 0.012f, 0.55f);
                Cube("Roof", skyline, new Vector3(x - 0.4f, h, 12), new Vector3(0.8f, 0.45f, 1),
                    "2e284e", 0.01f, 0.55f);
                for (int j = 0; j < 6; j++)
                {
                    Cube("Window", skyline, new Vector3(x - 0.65f + j % 3 * 0.56f,
                        h - 0.49f - j / 3 * 0.62f, 10.98f), new Vector3(0.18f, 0.22f, 0.03f),
                        (i + j) % 5 == 0 ? "b87e72" : "534561", 0, 0.75f);
                }
            }

            var market = Group("Milo's Market", Vector3.zero, root);
            Cube("Market wall", market, new Vector3(-7.05f, 3.45f, 2.72f),
                new Vector3(11.3f, 6.9f, 0.65f), "4c3666", 0.026f);
            Cube("Cornice", market, new Vector3(-7.02f, 6.72f, 2.32f),
                new Vector3(11.45f, 0.26f, 0.35f), "785481");
            for (int row = 0; row < 13; row++)
            {
                float y = row * 0.49f + 0.37f;
                Cube("Mortar", market, new Vector3(-7.05f, y, 2.385f),
                    new Vector3(11.3f, 0.023f, 0.01f), "3d2c53", 0);
                for (int col = 0; col < 9; col++)
                {
                    Cube("Mortar", market, new Vector3(-12.55f + col * 1.24f + row % 2 * 0.6f, y + 0.245f, 2.385f),
                        new Vector3(0.023f, 0.49f, 0.012f), "3d2c53", 0);
                }
            }

            Cube("Market sign", market, new Vector3(-7.5f, 4.83f, 2.21f),
                new Vector3(3.15f, 1.03f, 0.26f), "8f535b");
            Cube("Sign inset", market, new Vector3(-7.5f, 4.83f, 2.069f),
                new Vector3(2.97f, 0.86f, 0.04f), "3d2e46");
            Text(market, "MILO'S MARKET", new Vector3(-7.5f, 4.84f, 2.04f), 0.25f, "ffc18c");

            Cube("Doorframe", market, new Vector3(-3.72f, 1.48f, 2.27f),
                new Vector3(1.85f, 2.98f, 0.26f), "2b2338");
            Cube("Lit doorway", market, new Vector3(-3.72f, 1.43f, 2.105f),
                new Vector3(1.64f, 2.86f, 0.06f), "efad76", 0.01f, 1);
            Door = Group("Stockroom door pivot", new Vector3(-4.55f, 0, 2.0f), market);
            Cube("Door", Door, new Vector3(0.825f, 1.42f, 0), new Vector3(1.65f, 2.84f, 0.13f), "484659");
            Cube("Door plate", Door, new Vector3(0.83f, 2.17f, -0.083f),
                new Vector3(1.35f, 0.37f, 0.025f), "302a3e");
            Text(Door, "STOCKROOM", new Vector3(0.83f, 2.17f, -0.103f), 0.145f, "e8c8aa");
            Cube("Handle", Door, new Vector3(1.39f, 1.22f, -0.12f), new Vector3(0.21f, 0.08f, 0.1f), "d2a988");
            Cube("Lamp housing", market, new Vector3(-3.72f, 3.38f, 2.15f),
                new Vector3(0.52f, 0.19f, 0.39f), "332739");
            DoorLight = Cube("Stockroom light", market, new Vector3(-3.72f, 3.34f, 1.94f),
                new Vector3(0.43f, 0.11f, 0.035f), "ffe298", 0.01f, 1);
            DoorSpill = Group("Door light spill", new Vector3(-3.72f, 0.019f, 0.72f), root);
            MakeDoorSpill();

            var dumpster = Group("Dumpster", new Vector3(4.4f, 0, 0.95f), root);
            Shadow(root, 4.4f, 0.55f, 6.2f, 2.7f);
            Cube("Body", dumpster, new Vector3(0, 0.96f, 0), new Vector3(4.25f, 1.68f, 1.88f), "298b80", 0.043f);
            for (int i = -1; i <= 1; i++)
                Cube("Pressed rib", dumpster, new Vector3(i * 1.35f, 0.95f, -0.958f),
                    new Vector3(0.085f, 1.43f, 0.04f), "608578");
            Cube("Hinged lid", dumpster, new Vector3(0, 1.95f, -0.03f),
                new Vector3(4.46f, 0.27f, 2.04f), "335866", 0.034f).localRotation = Quaternion.Euler(-9, 0, 0);
            Cube("Label plate", dumpster, new Vector3(0, 1.27f, -1.013f),
                new Vector3(1.22f, 0.39f, 0.035f), "314753");
            Text(dumpster, "WASTE", new Vector3(0, 1.27f, -1.039f), 0.22f, "d3e0b6");

            for (int i = 0; i < 34; i++)
            {
                float x = (i * 39 % 100) * 0.22f - 10.5f;
                float z = (i * 17 % 40) * 0.15f - 3.9f;
                Cube("Pavement mark", root, new Vector3(x, 0.003f, z),
                    new Vector3(i % 3 * 0.12f + 0.08f, 0.005f, 0.024f), "665469", 0)
                    .localRotation = Quaternion.Euler(0, i * 33, 0);
            }

            Can = Group("Metal trash can", new Vector3(-7.95f, 0, -0.8f), root);
            Shadow(root, -7.6f, -0.32f, 2.8f, 1.53f);
            Part(PrimitiveType.Cylinder, "Can", Can, new Vector3(0, 0.63f, 0),
                new Vector3(1.24f, 0.63f, 1.24f), "8899b4", 0.03f);
            for (int i = 0; i < 16; i++)
            {
                float angle = i * Mathf.PI / 8;
                Cube("Rib", Can, new Vector3(0.615f * Mathf.Cos(angle), 0.6f, 0.615f * Mathf.Sin(angle)),
                    new Vector3(0.047f, 1.1f, 0.049f), "afc3d2", 0.007f);
            }

            CanLid = Group("Loose lid", new Vector3(0, 1.24f, 0), Can);
            Part(PrimitiveType.Cylinder, "Lid", CanLid, Vector3.zero,
                new Vector3(1.37f, 0.065f, 1.37f), "c5d9da", 0.03f);
            Cube("Lid grip", CanLid, new Vector3(0, 0.13f, 0),
                new Vector3(0.37f, 0.15f, 0.095f), "6d7889", 0.02f);
            AddComicScenery(root, market, dumpster, skyline);
            MakeRaccoon(root);
            MakeWorker(root);
        }

        internal static TextMesh Text(Transform parent, string message, Vector3 position, float size, string color)
        {
            var text = Group(message, position, parent).gameObject.AddComponent<TextMesh>();
            text.text = message;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
            text.fontSize = 54;
            text.characterSize = size / 5;
            text.fontStyle = FontStyle.Bold;
            text.color = Tint(color);
            return text;
        }

        public Transform Cardboard(Transform parent, Vector3 at, Vector3 size, int tone)
        {
            var key = (size, tone);
            if (!cartonTemplates.TryGetValue(key, out Transform template))
            {
                template = MakeCartonTemplate(size, tone);
                cartonTemplates.Add(key, template);
            }
            Transform box = Object.Instantiate(template, parent, false);
            box.name = "Carton";
            box.localPosition = at;
            return box;
        }

        private Transform MakeCartonTemplate(Vector3 size, int tone)
        {
            string[] colors = { "d7a06b", "bd7755", "e8ae72", "c88c60" };
            var box = Group("Carton", Vector3.zero, templateRoot);
            const float diagonal = 0.7071068f;
            float w = size.x * diagonal;
            float d = size.z * diagonal;
            float h = size.y * 0.5f;
            float bevel = 0.034f;
            Loft("Folded cardboard", box, Vector3.zero, colors[tone % colors.Length], 0.028f, 4,
                new(-h, w - bevel, d - bevel, 0), new(-h + bevel, w, d, 0),
                new(h - bevel, w, d, 0), new(h, w - bevel, d - bevel, 0));
            Cube("Packing tape", box, new Vector3(0, size.y * 0.508f, 0),
                new Vector3(size.x * 0.19f, 0.012f, size.z * 1.005f), "f0c891", 0);
            Cube("Label", box, new Vector3(size.x * 0.26f, -size.y * 0.17f, -size.z * 0.507f),
                new Vector3(size.x * 0.2f, size.y * 0.26f, 0.011f), "f4d5a5", 0);
            Cube("Handle mark", box, new Vector3(-size.x * 0.28f, -size.y * 0.18f, -size.z * 0.507f),
                new Vector3(size.x * 0.18f, size.y * 0.07f, 0.012f), "835a51", 0);
            Cube("Folded tape tab", box, new Vector3(0, h - 0.057f, -size.z * 0.504f),
                new Vector3(size.x * 0.19f, 0.11f, 0.012f), "f2be83", 0);
            for (int i = 0; i < 2; i++)
            {
                Cutout("This side up arrow", box, new Vector3(size.x * (-0.36f + i * 0.085f), 0.065f, -size.z * 0.507f),
                    "8e594c", new(-0.042f, 0.013f), new(0, 0.063f), new(0.042f, 0.013f),
                    new(0.013f, 0.014f), new(0.013f, -0.061f), new(-0.013f, -0.061f), new(-0.013f, 0.014f));
            }
            return box;
        }
    }
}
