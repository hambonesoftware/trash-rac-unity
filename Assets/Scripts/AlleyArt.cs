using System.Collections.Generic;
using UnityEngine;

namespace TrashRac
{
    internal sealed class AlleyArt
    {
        private readonly Shader shader = Resources.Load<Shader>("AlleyToon");
        private readonly Dictionary<(string, float, float), Material> materials = new();

        public Transform Door { get; private set; }
        public Transform DoorLight { get; private set; }
        public Transform DoorSpill { get; private set; }
        public Transform Worker { get; private set; }
        public Transform WorkerArm { get; private set; }
        public Transform WorkerBox { get; private set; }
        public Transform Raccoon { get; private set; }
        public Transform RaccoonArm { get; private set; }
        public Transform Can { get; private set; }

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
            if (!collider)
            {
                Collider component = part.GetComponent<Collider>();
                component.enabled = false;
                Object.Destroy(component);
            }

            var key = (color, ink, glow);
            if (!materials.TryGetValue(key, out Material material))
            {
                material = new Material(shader);
                material.SetColor("_Color", Tint(color));
                material.SetFloat("_Outline", ink);
                material.SetFloat("_Glow", glow);
                materials.Add(key, material);
            }

            part.GetComponent<Renderer>().sharedMaterial = material;
            return part.transform;
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
            Part(PrimitiveType.Cube, "Alley floor", root, new Vector3(0, -0.15f, 2),
                new Vector3(60, 0.3f, 38), "40364e", 0, 0.55f, true);

            var skyline = Group("Skyline", Vector3.zero, root);
            Part(PrimitiveType.Sphere, "Moon", skyline, new Vector3(10.3f, 9.4f, 14),
                Vector3.one * 1.35f, "ffd49a", 0.018f, 1);
            for (int i = 0; i < 32; i++)
            {
                float x = (i * 17 % 61) * 0.49f - 15;
                float y = 5.9f + (i * 13 % 29) * 0.17f;
                Part(PrimitiveType.Sphere, "Star", skyline, new Vector3(x, y, 17),
                    Vector3.one * (i % 5 == 0 ? 0.055f : 0.027f), "d8bcb5", 0, 1);
            }

            for (int i = 0; i < 13; i++)
            {
                float x = -15 + i * 2.35f;
                float h = 3 + (i * 7 % 6) * 0.42f;
                Cube("Building", skyline, new Vector3(x, h / 2, 12), new Vector3(2.15f, h, 2),
                    i % 2 == 0 ? "343048" : "3d374e", 0.012f, 0.55f);
                Cube("Roof", skyline, new Vector3(x - 0.4f, h, 12), new Vector3(0.8f, 0.45f, 1),
                    "343048", 0.01f, 0.55f);
                for (int j = 0; j < 6; j++)
                {
                    Cube("Window", skyline, new Vector3(x - 0.65f + j % 3 * 0.56f,
                        h - 0.49f - j / 3 * 0.62f, 10.98f), new Vector3(0.18f, 0.22f, 0.03f),
                        (i + j) % 5 == 0 ? "b87e72" : "534561", 0, 0.75f);
                }
            }

            var market = Group("Milo's Market", Vector3.zero, root);
            Cube("Market wall", market, new Vector3(-7.05f, 3.45f, 2.72f),
                new Vector3(11.3f, 6.9f, 0.65f), "524153", 0.026f);
            Cube("Cornice", market, new Vector3(-7.02f, 6.72f, 2.32f),
                new Vector3(11.45f, 0.26f, 0.35f), "705164");
            for (int row = 0; row < 13; row++)
            {
                float y = row * 0.49f + 0.37f;
                Cube("Mortar", market, new Vector3(-7.05f, y, 2.385f),
                    new Vector3(11.3f, 0.023f, 0.01f), "3f3448", 0);
                for (int col = 0; col < 9; col++)
                {
                    Cube("Mortar", market, new Vector3(-12.55f + col * 1.24f + row % 2 * 0.6f, y + 0.245f, 2.385f),
                        new Vector3(0.023f, 0.49f, 0.012f), "3f3448", 0);
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
            Part(PrimitiveType.Cylinder, "Warm spill", DoorSpill, Vector3.zero,
                new Vector3(2.7f, 0.006f, 3.65f), "896171", 0, 0.65f);

            var dumpster = Group("Dumpster", new Vector3(4.4f, 0, 0.95f), root);
            Shadow(root, 4.4f, 0.55f, 6.2f, 2.7f);
            Cube("Body", dumpster, new Vector3(0, 0.96f, 0), new Vector3(4.25f, 1.68f, 1.88f), "487367", 0.025f);
            for (int i = -1; i <= 1; i++)
                Cube("Pressed rib", dumpster, new Vector3(i * 1.35f, 0.95f, -0.958f),
                    new Vector3(0.085f, 1.43f, 0.04f), "608578");
            Cube("Hinged lid", dumpster, new Vector3(0, 1.95f, -0.03f),
                new Vector3(4.46f, 0.27f, 2.04f), "364d51", 0.027f).localRotation = Quaternion.Euler(-9, 0, 0);
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
                new Vector3(1.24f, 0.63f, 1.24f), "8994a0", 0.027f);
            for (int i = 0; i < 16; i++)
            {
                float angle = i * Mathf.PI / 8;
                Cube("Rib", Can, new Vector3(0.615f * Mathf.Cos(angle), 0.6f, 0.615f * Mathf.Sin(angle)),
                    new Vector3(0.047f, 1.1f, 0.049f), "bcc1bd", 0.006f);
            }

            Part(PrimitiveType.Cylinder, "Lid", Can, new Vector3(0, 1.26f, 0),
                new Vector3(1.37f, 0.065f, 1.37f), "b4bdbd", 0.026f);
            Cube("Lid grip", Can, new Vector3(0, 1.37f, 0),
                new Vector3(0.37f, 0.15f, 0.095f), "6d7889", 0.02f);
            MakeRaccoon(root);
            MakeWorker(root);
        }

        private static void Text(Transform parent, string message, Vector3 position, float size, string color)
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
        }

        private void MakeRaccoon(Transform root)
        {
            Raccoon = Group("Raccoon", Vector3.zero, root);
            Ball("Body", Raccoon, new Vector3(0, 0.58f, 0), new Vector3(0.8f, 1.14f, 0.65f), "8b8b9a");
            Ball("Belly", Raccoon, new Vector3(0.045f, 0.52f, -0.252f), new Vector3(0.56f, 0.71f, 0.23f), "c5b6ad");
            Ball("Head", Raccoon, new Vector3(0.06f, 1.17f, -0.07f), new Vector3(0.96f, 0.69f, 0.69f), "acabb7");
            for (int side = -1; side <= 1; side += 2)
            {
                Ball("Ear", Raccoon, new Vector3(0.06f + side * 0.31f, 1.49f, -0.08f),
                    new Vector3(0.29f, 0.38f, 0.25f), "56546a").localRotation = Quaternion.Euler(0, 0, side * -16);
                Ball("Inner ear", Raccoon, new Vector3(0.06f + side * 0.315f, 1.53f, -0.19f),
                    new Vector3(0.15f, 0.21f, 0.045f), "c89ea2");
                Ball("Mask", Raccoon, new Vector3(0.08f + side * 0.194f, 1.2f, -0.34f),
                    new Vector3(0.42f, 0.25f, 0.17f), "403a53").localRotation = Quaternion.Euler(0, 0, side * 14);
                Ball("Eye", Raccoon, new Vector3(0.108f + side * 0.18f, 1.229f, -0.415f),
                    new Vector3(0.125f, 0.139f, 0.062f), "fff0cd");
                Ball("Pupil", Raccoon, new Vector3(0.131f + side * 0.18f, 1.233f, -0.446f),
                    new Vector3(0.051f, 0.077f, 0.033f), "272337");
                Ball("Foot", Raccoon, new Vector3(side * 0.22f, 0.115f, -0.12f),
                    new Vector3(0.31f, 0.22f, 0.41f), "585367");
            }
            Ball("Snout", Raccoon, new Vector3(0.12f, 1.034f, -0.375f), new Vector3(0.43f, 0.26f, 0.3f), "e5cfc1");
            Ball("Nose", Raccoon, new Vector3(0.16f, 1.095f, -0.522f), new Vector3(0.22f, 0.12f, 0.13f), "33273b");
            for (int i = 0; i < 6; i++)
            {
                Ball("Tail stripe", Raccoon, new Vector3(-0.28f - i * 0.13f, 0.22f + i * i * 0.012f, 0.15f),
                    new Vector3(0.33f - i * 0.016f, 0.32f - i * 0.017f, 0.32f - i * 0.019f),
                    i % 2 == 0 ? "50495e" : "a39aa5");
            }
            RaccoonArm = Group("Throwing arm", new Vector3(0.32f, 0.8f, -0.1f), Raccoon);
            Ball("Sleeve", RaccoonArm, new Vector3(0.16f, 0, 0), new Vector3(0.44f, 0.23f, 0.25f), "78788b");
            Ball("Paw", RaccoonArm, new Vector3(0.36f, 0.02f, -0.04f), Vector3.one * 0.22f, "575164");
        }

        private void MakeWorker(Transform root)
        {
            Worker = Group("Stockroom worker", Vector3.zero, root);
            for (int side = -1; side <= 1; side += 2)
            {
                Ball("Trousers", Worker, new Vector3(side * 0.17f, 0.38f, 0), new Vector3(0.31f, 0.82f, 0.32f), "3e3a50");
                Ball("Boot", Worker, new Vector3(side * 0.17f, 0.11f, -0.11f), new Vector3(0.31f, 0.22f, 0.48f), "262435");
            }
            Ball("Apron", Worker, new Vector3(0, 0.98f, 0), new Vector3(0.73f, 0.85f, 0.48f), "b56558");
            Cube("Name badge", Worker, new Vector3(-0.15f, 1.16f, -0.23f), new Vector3(0.2f, 0.09f, 0.028f), "ffd49d");
            Ball("Head", Worker, new Vector3(0, 1.55f, -0.015f), new Vector3(0.49f, 0.58f, 0.46f), "d39779");
            Ball("Cap", Worker, new Vector3(0, 1.78f, 0), new Vector3(0.54f, 0.26f, 0.48f), "743f55");
            Ball("Brim", Worker, new Vector3(-0.05f, 1.72f, -0.15f), new Vector3(0.56f, 0.073f, 0.41f), "743f55");
            for (int side = -1; side <= 1; side += 2)
                Ball("Eye", Worker, new Vector3(side * 0.108f, 1.585f, -0.237f), Vector3.one * 0.052f, "3f2739");

            WorkerArm = Group("Throwing arm", new Vector3(-0.25f, 1.18f, -0.075f), Worker);
            Ball("Arm", WorkerArm, new Vector3(-0.23f, 0, 0), new Vector3(0.59f, 0.22f, 0.26f), "d39779");
            WorkerBox = Cardboard(Worker, new Vector3(0.23f, 1.05f, -0.38f), new Vector3(0.72f, 0.51f, 0.47f), 1);
        }

        public Transform Cardboard(Transform parent, Vector3 at, Vector3 size, int tone)
        {
            string[] colors = { "d7a16f", "bd7c56", "e2ae77", "c89061" };
            var box = Group("Carton", at, parent);
            Cube("Cardboard", box, Vector3.zero, size, colors[tone % colors.Length], 0.023f);
            Cube("Packing tape", box, new Vector3(0, size.y * 0.508f, 0),
                new Vector3(size.x * 0.19f, 0.012f, size.z * 1.005f), "f0c891", 0);
            Cube("Label", box, new Vector3(size.x * 0.26f, -size.y * 0.17f, -size.z * 0.507f),
                new Vector3(size.x * 0.2f, size.y * 0.26f, 0.011f), "f4d5a5", 0);
            Cube("Handle mark", box, new Vector3(-size.x * 0.28f, -size.y * 0.18f, -size.z * 0.507f),
                new Vector3(size.x * 0.18f, size.y * 0.07f, 0.012f), "835a51", 0);
            return box;
        }
    }
}
