using System.Collections.Generic;
using UnityEngine;

namespace TrashRac
{
    public sealed class AlleyGame : MonoBehaviour
    {
        private const int CartonLayer = 8;
        private static readonly Vector3 ThrowOrigin = new(-6.7f, 1.28f, 0);
        private static readonly Vector3 Peek = new(-7.04f, 0, -0.06f);
        private static readonly Vector3 Duck = new(-7.95f, 0, 0.07f);
        private static readonly Color Ink = AlleyArt.Tint("20192d");
        private static readonly Color Cream = AlleyArt.Tint("fff0d4");
        private static readonly Color Lime = AlleyArt.Tint("e3ed9f");
        private static readonly Color Peach = AlleyArt.Tint("ffad83");

        private sealed class Stack
        {
            public readonly float X;
            public readonly int StartCount;
            public readonly List<Transform> Cartons = new();
            public float Top;

            public Stack(float x, int startCount)
            {
                X = x;
                StartCount = startCount;
            }
        }

        private sealed class Projectile
        {
            public Transform Prop;
            public Vector3 From;
            public Vector3 Target;
            public Vector3 Last;
            public float Arc;
            public float Duration;
            public float Age;
        }

        private enum Phase { Ready, Playing, Won, Lost }

        private readonly Stack[] stacks = { new(-0.85f, 4), new(0.86f, 3), new(6.28f, 4), new(7.99f, 2) };
        private readonly Dictionary<Collider, (Stack stack, int level)> supports = new();
        private readonly Transform[] trail = new Transform[12];

        private AlleyArt art;
        private AlleyEffects effects;
        private Camera board;
        private Transform props;
        private Transform debris;
        private Phase phase;
        private Projectile shot;
        private Projectile incoming;
        private Vector3 aim = new(-0.85f, 0.28f, 0);
        private Vector3 lastPointer;
        private bool hidden;
        private bool alert;
        private int health;
        private int remaining;
        private int trashKind;
        private int workerStack;
        private float clock;
        private float nextVisit;
        private float visitStart = -1;
        private float flash;
        private float lidRattle;
        private float crouch;
        private float toss;
        private bool placed;
        private bool scanned;
        private string message;
        private Font font;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            board = AlleyArt.Group("Board camera", new Vector3(-0.48f, 7.7f, -22), transform).gameObject.AddComponent<Camera>();
            board.transform.LookAt(new Vector3(0, 2.95f, 0));
            board.orthographic = true;
            board.orthographicSize = 5.7f;
            board.clearFlags = CameraClearFlags.SolidColor;
            board.backgroundColor = AlleyArt.Tint("322942");
            board.nearClipPlane = 0.1f;
            board.farClipPlane = 90;

            art = new AlleyArt();
            art.Build(AlleyArt.Group("Back alley scenery", Vector3.zero, transform));
            effects = new AlleyEffects(art, AlleyArt.Group("Comic effects", Vector3.zero, transform));
            for (int i = 0; i < trail.Length; i++)
                trail[i] = art.Part(PrimitiveType.Sphere, "Aim dot", transform, Vector3.zero,
                    Vector3.one * (i % 3 == 0 ? 0.08f : 0.052f), "f4d7a3", 0, 1);

            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ResetBoard(Phase.Ready);
        }

        private void ResetBoard(Phase state)
        {
            if (props)
            {
                props.gameObject.SetActive(false);
                debris.gameObject.SetActive(false);
                Destroy(props.gameObject);
                Destroy(debris.gameObject);
            }
            props = AlleyArt.Group("Board pieces", Vector3.zero, transform);
            debris = AlleyArt.Group("Falling cardboard", Vector3.zero, transform);
            supports.Clear();
            health = 3;
            remaining = 0;
            clock = 0;
            nextVisit = 6.5f;
            visitStart = -1;
            flash = 0;
            lidRattle = 0;
            crouch = 0;
            toss = 0;
            hidden = false;
            shot = null;
            incoming = null;
            aim = new Vector3(-0.85f, 0.27f, 0);
            lastPointer = Input.mousePosition;
            foreach (Stack stack in stacks)
            {
                stack.Cartons.Clear();
                stack.Top = 0;
                for (int i = 0; i < stack.StartCount; i++) AddCarton(stack);
            }
            art.Raccoon.position = Peek;
            art.Raccoon.localScale = new Vector3(1.18f, 1.18f, 1.18f);
            art.PoseRaccoon(0, 0, 0, 0);
            art.CanLid.localRotation = Quaternion.identity;
            effects.Clear();
            art.Worker.gameObject.SetActive(false);
            SetDoor(false);
            phase = state;
            Say(state == Phase.Ready ? "The alley is quiet." : "Aim low to bring down a whole stack.");
            Physics.SyncTransforms();
        }

        private void AddCarton(Stack stack)
        {
            int level = stack.Cartons.Count;
            float[] heights = { 0.77f, 0.79f, 0.71f, 0.65f, 0.52f };
            float[] widths = { 1.53f, 1.34f, 1.43f, 1.16f, 0.92f };
            float[] shifts = { 0, 0.09f, -0.09f, 0.10f, -0.065f };
            var size = new Vector3(widths[level], heights[level], 0.98f - level * 0.05f);
            int tone = (System.Array.IndexOf(stacks, stack) + level) % 4;
            var box = art.Cardboard(props, new Vector3(stack.X + shifts[level], stack.Top + size.y / 2, 0), size, tone);
            box.localRotation = Quaternion.Euler(0, level % 2 == 0 ? -5 : 8, level % 2 == 0 ? 2 : -4);
            box.gameObject.layer = CartonLayer;
            var collider = box.gameObject.AddComponent<BoxCollider>();
            collider.size = size;
            supports.Add(collider, (stack, level));
            stack.Cartons.Add(box);
            stack.Top += size.y;
            remaining++;
        }

        private void Collapse(Stack stack, int first)
        {
            int count = stack.Cartons.Count - first;
            Vector3 impact = stack.Cartons[first].position;
            effects.Pop(AlleyEffects.Cue.Stack, impact + new Vector3(0.1f, 1.0f, -1.5f));
            effects.Scatter(impact, count);
            for (int i = stack.Cartons.Count - 1; i >= first; i--)
            {
                Transform box = stack.Cartons[i];
                var collider = box.GetComponent<BoxCollider>();
                stack.Top -= collider.size.y;
                supports.Remove(collider);
                box.gameObject.layer = 0;
                box.SetParent(debris, true);
                var body = box.gameObject.AddComponent<Rigidbody>();
                body.mass = 0.45f;
                body.AddForce(new Vector3(1.4f + (i - first) * 0.34f, 1.45f, -2.1f), ForceMode.VelocityChange);
                body.AddTorque(new Vector3(2.7f, (i % 2 == 0 ? 1 : -1) * 1.6f, -3.1f), ForceMode.VelocityChange);
                Destroy(box.gameObject, 2.0f);
            }
            stack.Cartons.RemoveRange(first, count);
            remaining -= count;
            if (visitStart < 0) Say(count == 1 ? "One carton down." : $"Stack down! {count} cartons fell together.");
            if (remaining == 0) End(Phase.Won);
        }

        private void Update()
        {
            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
            const float targetAspect = 16f / 9f;
            board.rect = aspect >= targetAspect
                ? new Rect((1 - targetAspect / aspect) * 0.5f, 0, targetAspect / aspect, 1)
                : new Rect(0, (1 - aspect / targetAspect) * 0.5f, 1, aspect / targetAspect);
            board.aspect = targetAspect;

            float dt = Mathf.Min(Time.deltaTime, 0.1f);
            effects.Update(dt);

            if (phase != Phase.Playing)
            {
                if (Input.GetKeyDown(KeyCode.Return)) ResetBoard(Phase.Playing);
                DrawAim();
                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetBoard(Phase.Playing);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space)) ToggleHide();
            UpdateAim();
            if (Input.GetMouseButtonDown(0) && CanAimAt(Input.mousePosition) || Input.GetKeyDown(KeyCode.Return))
                ThrowTrash();

            clock += dt;
            flash = Mathf.Max(0, flash - dt);
            lidRattle = Mathf.Max(0, lidRattle - dt);
            toss = Mathf.Max(0, toss - dt);
            UpdateRaccoon(dt);
            UpdateShot(dt);
            if (phase == Phase.Playing) UpdateWorker(dt);
            DrawAim();
        }

        private bool CanAimAt(Vector3 screen)
        {
            Rect pixels = board.pixelRect;
            return pixels.Contains(screen) && screen.y > pixels.y + pixels.height * 0.14f
                && screen.y < pixels.y + pixels.height * 0.86f;
        }

        private void UpdateAim()
        {
            Vector3 pointer = Input.mousePosition;
            if ((pointer != lastPointer || Input.GetMouseButtonDown(0)) && CanAimAt(pointer))
            {
                var plane = new Plane(Vector3.forward, Vector3.zero);
                Ray ray = board.ScreenPointToRay(pointer);
                if (plane.Raycast(ray, out float distance)) aim = ray.GetPoint(distance);
            }
            lastPointer = pointer;
            float step = 6 * Time.deltaTime;
            if (Input.GetKey(KeyCode.LeftArrow)) aim.x -= step;
            if (Input.GetKey(KeyCode.RightArrow)) aim.x += step;
            if (Input.GetKey(KeyCode.UpArrow)) aim.y += step;
            if (Input.GetKey(KeyCode.DownArrow)) aim.y -= step;
            aim.x = Mathf.Clamp(aim.x, -6.1f, 9.25f);
            aim.y = Mathf.Clamp(aim.y, 0.16f, 5.8f);
        }

        private void DrawAim()
        {
            bool visible = phase == Phase.Playing && !hidden;
            bool obstructed = false;
            for (int i = 0; i < trail.Length; i++)
            {
                if (visible && !obstructed)
                {
                    Vector3 point = Arc(ThrowOrigin, aim, 0.82f, (i + 1f) / (trail.Length + 1));
                    obstructed = Physics.CheckSphere(point, 0.045f, 1 << CartonLayer);
                    trail[i].position = point;
                }
                trail[i].gameObject.SetActive(visible && !obstructed);
            }
        }

        private void ThrowTrash()
        {
            if (hidden || shot != null) return;
            bool can = trashKind++ % 2 == 0;
            var prop = AlleyArt.Group(can ? "Empty soda can" : "Crumpled paper", ThrowOrigin, props);
            if (can)
            {
                art.Part(PrimitiveType.Cylinder, "Can body", prop, Vector3.zero, new Vector3(0.19f, 0.145f, 0.19f), "ef8178");
                art.Part(PrimitiveType.Cylinder, "Can tab", prop, new Vector3(0, 0.145f, 0),
                    new Vector3(0.189f, 0.016f, 0.189f), "ddd4c4");
            }
            else
            {
                art.Part(PrimitiveType.Cube, "Paper", prop, Vector3.zero, new Vector3(0.21f, 0.15f, 0.17f), "eddbb6");
            }
            shot = new Projectile
            {
                Prop = prop, From = ThrowOrigin, Target = aim, Last = ThrowOrigin, Arc = 0.82f,
                Duration = Mathf.Clamp(Vector3.Distance(ThrowOrigin, aim) / 19f, 0.22f, 0.82f)
            };
            toss = 0.25f;
        }

        private static Vector3 Arc(Vector3 from, Vector3 target, float height, float t)
        {
            return Vector3.Lerp(from, target, t) + Vector3.up * (height * 4 * t * (1 - t));
        }

        private static Vector3 MoveProjectile(Projectile projectile, float dt)
        {
            projectile.Age += dt;
            float t = Mathf.Clamp01(projectile.Age / projectile.Duration);
            Vector3 p = Arc(projectile.From, projectile.Target, projectile.Arc, t);
            projectile.Prop.SetPositionAndRotation(p, Quaternion.Euler(t * 210, t * 175, t * 410));
            return p;
        }

        private void UpdateShot(float dt)
        {
            if (shot == null) return;
            Vector3 point = MoveProjectile(shot, dt);
            Vector3 travel = point - shot.Last;
            if (Physics.SphereCast(shot.Last, 0.065f, travel.normalized, out RaycastHit hit,
                travel.magnitude, 1 << CartonLayer) && supports.TryGetValue(hit.collider, out var support))
            {
                Destroy(shot.Prop.gameObject);
                shot = null;
                Collapse(support.stack, support.level);
            }
            else if (shot.Age >= shot.Duration)
            {
                Destroy(shot.Prop.gameObject);
                shot = null;
            }
            else
            {
                shot.Last = point;
            }
        }

        private void ToggleHide()
        {
            hidden = !hidden;
            if (hidden)
            {
                lidRattle = 0.31f;
                effects.Pop(AlleyEffects.Cue.Duck, art.Can.position + new Vector3(-0.15f, 1.58f, -0.36f));
            }
            if (incoming != null)
                Say(hidden ? "Stay down until the box passes." : "Incoming box! Hide!", !hidden);
            else if (visitStart >= 0)
                Say(hidden ? "Behind cover. Wait for the worker to leave." : "Worker in the alley. Get ready to duck.", !hidden);
            else
                Say(hidden ? "Behind cover. Peek out to throw." : "Out of cover. Line up your throw.");
        }

        private void UpdateRaccoon(float dt)
        {
            float blend = 1 - Mathf.Exp(-22 * dt);
            crouch = Mathf.Lerp(crouch, hidden ? 1 : 0, blend);
            art.Raccoon.position = Vector3.Lerp(art.Raccoon.position, hidden ? Duck : Peek, blend);
            art.Raccoon.localScale = new Vector3(1.18f + 0.14f * crouch, 1.18f - 0.58f * crouch, 1.18f);
            float throwPose = Mathf.SmoothStep(0, 1, toss / 0.16f) * (1 - crouch);
            art.PoseRaccoon(clock, crouch, throwPose, flash / 0.28f);
            art.CanLid.localRotation = Quaternion.Euler(0, 0, lidRattle * Mathf.Sin(lidRattle * 61) * 35);
        }

        private void SetDoor(bool open)
        {
            art.Door.localRotation = Quaternion.Euler(0, open ? -83 : 0, 0);
            art.DoorLight.gameObject.SetActive(open);
            art.DoorSpill.gameObject.SetActive(open);
            Shader.SetGlobalFloat("_StockroomWake", open ? 1 : 0);
        }

        private void BeginVisit()
        {
            visitStart = clock;
            workerStack = 0;
            for (int i = 1; i < stacks.Length; i++)
            {
                int gap = stacks[i].StartCount - stacks[i].Cartons.Count;
                int chosenGap = stacks[workerStack].StartCount - stacks[workerStack].Cartons.Count;
                if (gap > chosenGap || gap == chosenGap && stacks[i].Cartons.Count < stacks[workerStack].Cartons.Count)
                    workerStack = i;
            }
            placed = false;
            scanned = false;
            SetDoor(true);
            art.WorkerBox.gameObject.SetActive(true);
            Say("Stockroom door! A worker is coming out.", true);
        }

        private void UpdateWorker(float dt)
        {
            if (visitStart < 0 && clock >= nextVisit) BeginVisit();
            if (visitStart < 0) return;

            const float leave = 3.3f;
            const float end = 4.7f;
            const float check = 2.12f;
            const float throwAt = 2.70f;
            float age = clock - visitStart;
            float doorSwing = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0, 0.27f, age))
                * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(end, end - 0.26f, age));
            art.Door.localRotation = Quaternion.Euler(0, -83 * doorSwing, 0);
            float walk = age < 1.5f ? Mathf.InverseLerp(0.32f, 1.5f, age) : 1 - Mathf.InverseLerp(leave, end, age);
            float stride = Mathf.Sin(walk * Mathf.PI);
            Stack stack = stacks[workerStack];
            Vector3 doorway = new(-3.72f, 0, 1.73f);
            Vector3 drop = new(stack.X - 0.92f, 0, -0.82f);
            art.Worker.position = Vector3.Lerp(doorway, drop, walk)
                + Vector3.up * (Mathf.Abs(Mathf.Sin(age * 15)) * stride * 0.084f);
            art.Worker.gameObject.SetActive(age >= 0.28f);
            art.Worker.localRotation = Quaternion.Euler(0, age < 1.5f ? -32 : 23, 0);
            bool preparing = scanned && art.WorkerBox.gameObject.activeSelf;
            float windup = preparing ? Mathf.SmoothStep(0, 1, Mathf.InverseLerp(check, throwAt - 0.08f, age)) : 0;
            float release = preparing && age >= throwAt ? 1 : incoming == null ? 0 : Mathf.Clamp01(1 - (age - throwAt) * 5);
            float lift = Mathf.Sin(Mathf.InverseLerp(1.31f, 1.78f, age) * Mathf.PI);
            art.PoseWorker(age, stride, lift, windup, release, !placed, age >= 1.5f && age < leave);

            if (!placed && age >= 1.5f)
            {
                placed = true;
                if (stack.Cartons.Count < stack.StartCount + 1)
                {
                    AddCarton(stack);
                    Physics.SyncTransforms();
                }
                art.WorkerBox.gameObject.SetActive(false);
                Say(hidden ? "Stay low. The worker is stacking cardboard." : "The worker is looking over. Hide!", !hidden);
            }

            if (!scanned && age >= check)
            {
                scanned = true;
                if (!hidden)
                {
                    art.WorkerBox.gameObject.SetActive(true);
                    effects.Pop(AlleyEffects.Cue.Spotted, art.Worker.position + new Vector3(0, 2.6f, -0.6f));
                    Say("Spotted! Duck before the box lands!", true);
                }
            }

            if (age >= throwAt && art.WorkerBox.gameObject.activeSelf)
            {
                art.WorkerBox.gameObject.SetActive(false);
                Vector3 from = art.WorkerBox.position;
                incoming = new Projectile
                {
                    From = from, Target = new Vector3(-7.04f, 0.95f, -0.43f), Arc = 0.93f, Duration = 0.61f,
                    Prop = art.Cardboard(props, from, new Vector3(0.75f, 0.53f, 0.45f), 2)
                };
            }

            if (incoming != null)
            {
                Vector3 destination = hidden ? art.Can.position + new Vector3(0.23f, 1.13f, 0.07f)
                    : new Vector3(-7.04f, 0.95f, -0.43f);
                incoming.Target = Vector3.Lerp(incoming.Target, destination, Mathf.Clamp01(dt * 15));
                MoveProjectile(incoming, dt);
                if (incoming.Age >= incoming.Duration)
                {
                    Destroy(incoming.Prop.gameObject);
                    incoming = null;
                    if (hidden)
                    {
                        lidRattle = 0.58f;
                        effects.Pop(AlleyEffects.Cue.Cover, art.Can.position + new Vector3(0.62f, 1.65f, -0.55f));
                        Say("Clang! The trash can blocked the box.");
                    }
                    else
                    {
                        flash = 0.28f;
                        effects.Pop(AlleyEffects.Cue.Hit, destination + new Vector3(0.25f, 1.02f, -0.55f));
                        health--;
                        Say("Ouch! A box hit you.", true);
                        if (health == 0) End(Phase.Lost);
                    }
                }
            }

            if (age < end) return;
            art.Worker.gameObject.SetActive(false);
            SetDoor(false);
            visitStart = -1;
            nextVisit = clock + 5.8f;
            Say(hidden ? "All clear. Peek out to throw." : "The worker went inside. Keep throwing.");
        }

        private void End(Phase outcome)
        {
            phase = outcome;
            Say(outcome == Phase.Won ? "Dinner is served." : "The alley is closed.");
        }

        private void Say(string text, bool urgent = false)
        {
            message = text;
            alert = urgent;
        }

        private static void Panel(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        private void Text(Rect rect, string value, int size, Color color, TextAnchor align = TextAnchor.MiddleLeft)
        {
            GUI.Label(rect, value, new GUIStyle(GUI.skin.label)
            {
                font = font, fontSize = size, fontStyle = FontStyle.Bold, alignment = align,
                wordWrap = false, normal = { textColor = color }
            });
        }

        private bool Button(Rect rect, string value, bool primary = false)
        {
            bool pressed = GUI.Button(rect, GUIContent.none, GUIStyle.none);
            Color fill = primary ? Lime : AlleyArt.Tint("3e354b");
            if (rect.Contains(Event.current.mousePosition)) fill = Color.Lerp(fill, Color.white, 0.18f);
            Panel(rect, fill);
            Text(rect, value, 18, primary ? Ink : Cream, TextAnchor.MiddleCenter);
            return pressed;
        }

        private void OnGUI()
        {
            if (!board) return;
            Rect pixels = board.pixelRect;
            var oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(pixels.x, Screen.height - pixels.yMax, 0), Quaternion.identity,
                new Vector3(pixels.width / 1280, pixels.height / 720, 1));

            Panel(new Rect(0, 0, 1280, 65), Ink);
            Panel(new Rect(0, 64, 1280, 2), AlleyArt.Tint("725465"));
            Text(new Rect(26, 5, 220, 51), "TRASH RAC", 29, Lime);
            Text(new Rect(250, 5, 325, 51), "01   BACK ALLEY RUMBLE", 19, Cream);
            Text(new Rect(685, 6, 90, 50), "HEALTH", 17, Cream);
            for (int i = 0; i < 3; i++) Panel(new Rect(776 + i * 19, 26, 13, 14), i < health ? Peach : AlleyArt.Tint("584252"));
            Text(new Rect(864, 6, 233, 50), $"CARTONS   {remaining:00}", 19, Cream);
            if (Button(new Rect(1110, 14, 143, 36), "RESTART  R")) ResetBoard(Phase.Playing);

            Panel(new Rect(0, 657, 1280, 63), Ink);
            Panel(new Rect(0, 656, 1280, 2), AlleyArt.Tint("725465"));
            Text(new Rect(26, 662, 797, 51), message, 21, alert ? Peach : Cream);
            if (phase == Phase.Playing && Button(new Rect(1045, 668, 208, 39), hidden ? "PEEK   SPACE" : "HIDE   SPACE", hidden))
                ToggleHide();

            if (phase == Phase.Playing && !hidden)
            {
                Vector3 screen = board.WorldToViewportPoint(aim);
                float x = screen.x * 1280;
                float y = (1 - screen.y) * 720;
                Panel(new Rect(x - 9, y - 1, 19, 2), Lime);
                Panel(new Rect(x - 1, y - 9, 2, 19), Lime);
            }

            if (phase != Phase.Playing) ShowCard();
            GUI.matrix = oldMatrix;
        }

        private void ShowCard()
        {
            Panel(new Rect(0, 66, 1280, 590), new Color(0.055f, 0.035f, 0.09f, 0.47f));
            Panel(new Rect(335, 191, 610, 335), AlleyArt.Tint("78576d"));
            Panel(new Rect(337, 193, 606, 331), Ink);
            string eyebrow = phase == Phase.Ready ? "THE MIDNIGHT RAID" : phase == Phase.Won ? "RAID COMPLETE" : "BUSTED";
            string title = phase == Phase.Ready ? "Dinner’s behind the cardboard." : phase == Phase.Won ? "Dumpster unlocked." : "Boxed out.";
            string firstLine = phase == Phase.Ready ? "Knock down the stacks to reach the dumpster."
                : phase == Phase.Won ? "Leftover pizza is officially on the menu." : "The stock crew caught you in the open.";
            string secondLine = phase == Phase.Ready ? "Duck when the stockroom door lights up."
                : phase == Phase.Won ? "The midnight raid was a success." : "The can can block a box even after it’s thrown.";
            Text(new Rect(350, 212, 580, 30), eyebrow, 19, Peach, TextAnchor.MiddleCenter);
            Text(new Rect(350, 254, 580, 53), title, 33, Cream, TextAnchor.MiddleCenter);
            Text(new Rect(350, 325, 580, 26), firstLine, 20, Cream, TextAnchor.MiddleCenter);
            Text(new Rect(350, 354, 580, 26), secondLine, 20, Cream, TextAnchor.MiddleCenter);
            if (Button(new Rect(523, 407, 234, 51), phase == Phase.Ready ? "PLAY BOARD" : "PLAY AGAIN", true))
                ResetBoard(Phase.Playing);
            Text(new Rect(350, 475, 580, 28), "CLICK TO THROW  /  SPACE TO DUCK  /  ARROWS + ENTER", 15, Peach, TextAnchor.MiddleCenter);
        }
    }
}
