using UnityEngine;

namespace TrashRac
{
    internal sealed partial class AlleyArt
    {
        private Transform raccoonTorso, raccoonHead, raccoonElbow, raccoonTail;
        private readonly Transform[] eyes = new Transform[2];
        private readonly Transform[] tail = new Transform[5];
        private Transform workerTorso, workerHead, workerElbow;
        private readonly Transform[] workerLegs = new Transform[2];
        private readonly Transform[] workerKnees = new Transform[2];
        private readonly Transform[] workerBoots = new Transform[2];
        private Transform workerCarryHand, workerThrowHand;

        private Transform MeshPart(string name, Transform parent, Vector3 position, Mesh mesh, string color, float ink, float glow = 0)
        {
            Transform part = Group(name, position, parent);
            part.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            part.gameObject.AddComponent<MeshRenderer>().sharedMaterial = MaterialFor(color, ink, glow);
            return part;
        }

        private Transform Loft(string name, Transform parent, Vector3 position, string color, float ink,
            int sides, params Vector4[] rings)
        {
            var vertices = new Vector3[rings.Length * sides];
            var triangles = new int[((rings.Length - 1) * sides * 2 + (sides - 2) * 2) * 3];
            int index = 0;
            for (int ring = 0; ring < rings.Length; ring++)
            {
                Vector4 section = rings[ring];
                for (int i = 0; i < sides; i++)
                {
                    float theta = (i + 0.5f) * Mathf.PI * 2 / sides;
                    int a = ring * sides + i;
                    vertices[a] = new Vector3(Mathf.Cos(theta) * section.y, section.x,
                        section.w + Mathf.Sin(theta) * section.z);
                    if (ring == 0) continue;
                    int b = ring * sides + (i + 1) % sides;
                    triangles[index++] = a - sides;
                    triangles[index++] = a;
                    triangles[index++] = b - sides;
                    triangles[index++] = b - sides;
                    triangles[index++] = a;
                    triangles[index++] = b;
                }
            }
            int top = (rings.Length - 1) * sides;
            for (int i = 1; i < sides - 1; i++)
            {
                triangles[index++] = 0;
                triangles[index++] = i;
                triangles[index++] = i + 1;
                triangles[index++] = top;
                triangles[index++] = top + i + 1;
                triangles[index++] = top + i;
            }
            var mesh = new Mesh { name = name, vertices = vertices, triangles = triangles };
            mesh.RecalculateNormals();
            return MeshPart(name, parent, position, mesh, color, ink);
        }

        internal Transform Cutout(string name, Transform parent, Vector3 position, string color, params Vector2[] corners)
        {
            var vertices = new Vector3[corners.Length + 1];
            for (int i = 0; i < corners.Length; i++)
            {
                vertices[i + 1] = corners[i];
                vertices[0] += vertices[i + 1] / corners.Length;
            }
            var triangles = new int[corners.Length * 3];
            for (int i = 0; i < corners.Length; i++)
            {
                int a = i + 1;
                int b = (i + 1) % corners.Length + 1;
                bool clockwise = Vector3.Cross(vertices[a] - vertices[0], vertices[b] - vertices[0]).z < 0;
                triangles[3 * i + 1] = clockwise ? a : b;
                triangles[3 * i + 2] = clockwise ? b : a;
            }
            var mesh = new Mesh { name = name, vertices = vertices, triangles = triangles };
            mesh.RecalculateNormals();
            return MeshPart(name, parent, position, mesh, color, 0, 1);
        }

        private void MakeRaccoon(Transform root)
        {
            Raccoon = Group("Raccoon rig", Vector3.zero, root);
            raccoonTorso = Group("Chest pivot", new Vector3(0, 0.42f, 0), Raccoon);
            Loft("Tapered torso", raccoonTorso, Vector3.zero, "7c83a1", 0.031f, 10,
                new(-0.18f, 0.23f, 0.23f, 0.04f), new(0.02f, 0.35f, 0.27f, 0),
                new(0.35f, 0.38f, 0.31f, -0.03f), new(0.59f, 0.29f, 0.25f, -0.04f));
            Cutout("Cream chest", raccoonTorso, new Vector3(0.018f, 0.25f, -0.304f), "ded1c2",
                new(-0.22f, 0.13f), new(-0.1f, -0.2f), new(0.10f, -0.24f), new(0.23f, 0.13f), new(0.13f, 0.29f));
            Loft("Green neckerchief", raccoonTorso, new Vector3(0, 0.53f, -0.02f), "78bb8a", 0.019f, 10,
                new(-0.07f, 0.285f, 0.29f, -0.025f), new(0.08f, 0.29f, 0.27f, 0));
            Cutout("Neckerchief point", raccoonTorso, new Vector3(0.01f, 0.47f, -0.339f), "a4d48e",
                new(-0.22f, 0.08f), new(0.22f, 0.06f), new(0.04f, -0.29f));
            raccoonHead = Group("Head pivot", new Vector3(0.04f, 0.77f, -0.085f), raccoonTorso);
            Loft("Angular head", raccoonHead, Vector3.zero, "a4a8ba", 0.035f, 10,
                new(-0.21f, 0.22f, 0.24f, -0.04f), new(-0.12f, 0.47f, 0.31f, 0),
                new(0.065f, 0.46f, 0.33f, 0), new(0.26f, 0.32f, 0.25f, 0.035f));
            for (int i = 0; i < 2; i++)
            {
                int side = i == 0 ? -1 : 1;
                Transform ear = Loft("Pointed ear", raccoonHead, new Vector3(side * 0.315f, 0.29f, 0.01f),
                    "6d7592", 0.024f, 7, new(-0.095f, 0.15f, 0.11f, 0),
                    new(0.055f, 0.112f, 0.089f, 0.025f), new(0.215f, 0.017f, 0.018f, 0.02f));
                ear.localRotation = Quaternion.Euler(-7, 0, side * -18);
                Cutout("Ear lining", ear, new Vector3(0, 0.007f, -0.107f), "ca8c9c",
                    new(-0.087f, -0.06f), new(0.091f, -0.06f), new(0, 0.136f));
                Cutout("Cheek tufts", raccoonHead, new Vector3(side * 0.33f, -0.07f, -0.262f), "c5c2c8",
                    new(-side * 0.10f, 0.12f), new(side * 0.24f, 0.06f), new(side * 0.13f, -0.01f),
                    new(side * 0.18f, -0.065f), new(-side * 0.11f, -0.13f));
                Transform mask = Cutout("Angular eye mask", raccoonHead, new Vector3(side * 0.185f, 0.015f, -0.304f),
                    "302b4a", new(-0.21f, 0.075f), new(0.19f, 0.105f), new(0.237f, 0.006f),
                    new(0.10f, -0.104f), new(-0.144f, -0.101f));
                mask.localRotation = Quaternion.Euler(0, -side * 21, side * 9);
                Transform eye = Group("Eye and pupil", new Vector3(side * 0.185f + 0.025f, 0.029f, -0.343f), raccoonHead);
                eyes[i] = eye;
                Cutout("Sclera", eye, Vector3.zero, "fff0c9", new(-0.116f, 0.034f), new(0.113f, 0.061f),
                    new(0.097f, -0.043f), new(-0.055f, -0.051f));
                Ball("Pupil", eye, new Vector3(0.048f, 0, -0.011f), new Vector3(0.062f, 0.091f, 0.034f), "25213a");
                Cutout("Expressive brow", raccoonHead, new Vector3(side * 0.185f, 0.103f, -0.338f),
                    "302641", new(-0.145f, -0.014f), new(0.124f, -0.004f), new(0.105f, 0.046f), new(-0.08f, 0.062f));
                Loft("Paw boot", Raccoon, new Vector3(side * 0.19f, 0.14f, -0.087f), "494461", 0.022f, 8,
                    new(-0.125f, 0.155f, 0.205f, -0.045f), new(-0.02f, 0.17f, 0.22f, -0.04f),
                    new(0.115f, 0.11f, 0.14f, 0.025f));
            }
            Loft("Cream snout", raccoonHead, new Vector3(0.055f, -0.114f, -0.265f), "ead3be", 0.019f, 8,
                new(-0.099f, 0.105f, 0.109f, -0.067f), new(-0.017f, 0.205f, 0.17f, -0.036f),
                new(0.09f, 0.139f, 0.153f, -0.044f));
            Ball("Nose", raccoonHead, new Vector3(0.126f, -0.083f, -0.471f),
                new Vector3(0.198f, 0.105f, 0.13f), "302743");
            raccoonTail = Group("Tail pivot", new Vector3(-0.17f, 0.31f, 0.22f), Raccoon);
            Transform joint = raccoonTail;
            for (int i = 0; i < tail.Length; i++)
            {
                Transform segment = Group("Tail joint", i == 0 ? Vector3.zero : new Vector3(0, 0.235f, 0), joint);
                float radius = new[] { 0.13f, 0.177f, 0.201f, 0.176f, 0.11f }[i];
                Loft("Tail stripe", segment, new Vector3(0, 0.09f, 0), i % 2 == 0 ? "595473" : "a6a2b1", 0.014f, 9,
                    new(-0.115f, radius * 0.81f, radius * 0.77f, 0), new(0, radius, radius * 0.86f, 0),
                    new(0.156f, i == tail.Length - 1 ? 0.019f : radius * 0.9f, radius * 0.65f, 0));
                tail[i] = segment;
                joint = segment;
            }
            Transform left = Group("Support arm", new Vector3(-0.29f, 0.39f, 0.035f), raccoonTorso);
            MakeSleeve(left, "70718f", 0.36f, 0.095f);
            left.localRotation = Quaternion.Euler(0, 0, -34);
            RaccoonArm = Group("Throwing shoulder", new Vector3(0.29f, 0.38f, -0.05f), raccoonTorso);
            MakeSleeve(RaccoonArm, "7f83a0", 0.32f, 0.112f);
            raccoonElbow = Group("Throwing elbow", new Vector3(0, -0.295f, 0), RaccoonArm);
            MakeSleeve(raccoonElbow, "615f80", 0.26f, 0.093f);
            Ball("Throwing paw", raccoonElbow, new Vector3(0, -0.26f, -0.024f),
                new Vector3(0.224f, 0.216f, 0.22f), "4c4262");
        }

        private void MakeSleeve(Transform parent, string color, float length, float radius)
        {
            Loft("Sleeve", parent, new Vector3(0, -length * 0.5f, 0), color, 0.022f, 8,
                new(-length * 0.55f, radius * 0.8f, radius * 0.85f, 0),
                new(0, radius, radius, 0), new(length * 0.52f, radius * 0.88f, radius, 0));
        }

        public void PoseRaccoon(float clock, float crouch, float throwPose, float hit)
        {
            float breath = Mathf.Sin(clock * 5.7f);
            raccoonTorso.localRotation = Quaternion.Euler(-14 * crouch, 0, -12 * crouch - 13 * throwPose);
            raccoonHead.localRotation = Quaternion.Euler(7 * crouch, -12 - 10 * throwPose, -5 + breath * 1.7f + hit * 27);
            float blink = Mathf.SmoothStep(0.19f, 1, Mathf.Abs(Mathf.Repeat(clock + 1, 3.1f) - 1.55f) * 17);
            foreach (Transform eye in eyes)
                eye.localScale = new Vector3(1, blink * (1 - 0.55f * crouch), 1);
            RaccoonArm.localRotation = Quaternion.Euler(13 + 10 * crouch, -12, 32 + throwPose * 108 - crouch * 38);
            raccoonElbow.localRotation = Quaternion.Euler(0, 0, 37 - 34 * throwPose);
            raccoonTail.localRotation = Quaternion.Euler(10, -10, 75 + 16 * crouch + 8 * breath);
            for (int i = 0; i < tail.Length; i++)
                tail[i].localRotation = Quaternion.Euler(0, 0, -13 + Mathf.Sin(clock * 7 - i * 0.63f) * 7 - 6 * throwPose);
        }

        private void MakeWorker(Transform root)
        {
            Worker = Group("Stock clerk rig", Vector3.zero, root);
            workerTorso = Group("Apron and shoulders", new Vector3(0, 0.91f, 0), Worker);
            Loft("Boxy work shirt", workerTorso, Vector3.zero, "c5795d", 0.027f, 8,
                new(-0.15f, 0.35f, 0.26f, 0.03f), new(0.23f, 0.44f, 0.33f, 0),
                new(0.53f, 0.48f, 0.31f, 0.02f), new(0.64f, 0.32f, 0.23f, 0.01f));
            Cutout("Apron", workerTorso, new Vector3(0, 0.12f, -0.312f), "683c59",
                new(-0.39f, -0.32f), new(0.39f, -0.32f), new(0.28f, 0.23f), new(0.22f, 0.43f),
                new(-0.22f, 0.43f), new(-0.28f, 0.23f));
            Cube("Apron pocket", workerTorso, new Vector3(0.025f, 0.024f, -0.34f),
                new Vector3(0.37f, 0.19f, 0.026f), "966076");
            Cube("Name tag", workerTorso, new Vector3(-0.14f, 0.442f, -0.32f),
                new Vector3(0.19f, 0.076f, 0.026f), "fff0bd");
            workerHead = Group("Head pivot", new Vector3(0, 0.86f, -0.01f), workerTorso);
            Loft("Square jaw", workerHead, Vector3.zero, "d49a75", 0.026f, 8,
                new(-0.25f, 0.193f, 0.21f, -0.04f), new(-0.124f, 0.272f, 0.238f, -0.016f),
                new(0.153f, 0.278f, 0.25f, 0), new(0.23f, 0.19f, 0.195f, 0));
            Loft("Stockroom cap", workerHead, new Vector3(0.01f, 0.20f, 0.015f), "a74d61", 0.027f, 9,
                new(-0.06f, 0.29f, 0.272f, 0), new(0.06f, 0.284f, 0.257f, 0),
                new(0.175f, 0.15f, 0.17f, 0.025f));
            Ball("Cap visor", workerHead, new Vector3(-0.025f, 0.149f, -0.194f),
                new Vector3(0.55f, 0.083f, 0.43f), "723951");
            Ball("Nose", workerHead, new Vector3(-0.03f, -0.021f, -0.239f),
                new Vector3(0.16f, 0.146f, 0.192f), "c98368");
            for (int i = 0; i < 2; i++)
            {
                int side = i == 0 ? -1 : 1;
                Ball("Eye", workerHead, new Vector3(side * 0.108f, 0.035f, -0.23f),
                    new Vector3(0.091f, 0.093f, 0.059f), "fff3d2");
                Ball("Pupil", workerHead, new Vector3(side * 0.108f - 0.017f, 0.039f, -0.263f),
                    Vector3.one * 0.041f, "382740");
                Transform hip = Group("Hip", new Vector3(side * 0.19f, 0.84f, 0.01f), Worker);
                workerLegs[i] = hip;
                MakeSleeve(hip, "454767", 0.43f, 0.146f);
                Transform knee = Group("Knee", new Vector3(0, -0.39f, 0), hip);
                workerKnees[i] = knee;
                MakeSleeve(knee, "3a3e5a", 0.34f, 0.13f);
                Transform boot = Group("Work boot", new Vector3(0, -0.335f, -0.086f), knee);
                workerBoots[i] = boot;
                Loft("Scuffed shoe", boot, Vector3.zero, "312d48", 0.022f, 8,
                    new(-0.11f, 0.17f, 0.225f, -0.017f), new(0.01f, 0.179f, 0.228f, -0.022f),
                    new(0.104f, 0.145f, 0.154f, 0.03f));
            }
            Transform carryArm = Group("Carry arm", new Vector3(0.4f, 0.45f, 0), workerTorso);
            MakeSleeve(carryArm, "b26758", 0.37f, 0.115f);
            Transform carryElbow = Group("Carry elbow", new Vector3(0, -0.33f, 0), carryArm);
            MakeSleeve(carryElbow, "d39876", 0.33f, 0.095f);
            workerCarryHand = Group("Carry hand", new Vector3(0, -0.27f, 0), carryElbow);
            Ball("Carry hand", workerCarryHand, Vector3.zero, Vector3.one * 0.23f, "e0a982");
            carryArm.localRotation = Quaternion.Euler(11, 0, -21);
            carryElbow.localRotation = Quaternion.Euler(80, 4, -26);
            WorkerArm = Group("Throwing shoulder", new Vector3(-0.41f, 0.46f, -0.02f), workerTorso);
            MakeSleeve(WorkerArm, "c4785c", 0.37f, 0.117f);
            workerElbow = Group("Throwing elbow", new Vector3(0, -0.33f, 0), WorkerArm);
            MakeSleeve(workerElbow, "d49a75", 0.31f, 0.097f);
            workerThrowHand = Group("Throwing hand", new Vector3(0, -0.28f, 0), workerElbow);
            Ball("Throwing hand", workerThrowHand, Vector3.zero, Vector3.one * 0.234f, "e0a982");
            WorkerBox = Cardboard(Worker, Vector3.zero, new Vector3(0.68f, 0.49f, 0.46f), 1);
        }

        public void PoseWorker(float age, float stride, float lift, float windup, float release, bool carrying, bool looking)
        {
            float step = Mathf.Sin(age * 15) * stride;
            for (int i = 0; i < 2; i++)
            {
                float swing = i == 0 ? step : -step;
                float knee = Mathf.Max(0, -swing) * 29;
                workerLegs[i].localRotation = Quaternion.Euler(swing * 24, 0, swing * 8);
                workerKnees[i].localRotation = Quaternion.Euler(knee, 0, 0);
                workerBoots[i].localRotation = Quaternion.Euler(-knee * 0.6f, 0, -swing * 8);
            }
            workerTorso.localRotation = Quaternion.Euler(lift * 23, windup * 32, -windup * 11 + release * 19);
            workerHead.localRotation = Quaternion.Euler(lift * 11, looking ? -36 : step * 7, -windup * 9);
            WorkerArm.localRotation = Quaternion.Euler(carrying ? 43 : 3 - windup * 61,
                -18 * windup, -21 - windup * 64 - release * 52);
            workerElbow.localRotation = Quaternion.Euler(carrying ? 32 : 14 + windup * 52, 0, -20 * windup);
            WorkerBox.position = carrying ? (workerCarryHand.position + workerThrowHand.position) * 0.5f + new Vector3(0, 0.14f, -0.18f)
                : workerThrowHand.position + new Vector3(0, 0.12f, -0.085f);
            WorkerBox.rotation = Quaternion.Euler(windup * -23, windup * 20, carrying ? -7 : windup * -25);
        }
    }
}
