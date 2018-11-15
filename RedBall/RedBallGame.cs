using System.Diagnostics;
using System.Threading.Tasks;
using Urho;
using Urho.Physics;
using Urho.Shapes;

namespace RedBall
{
    public class RedBallGame: Application
    {
        Scene scene;

        public Player Player { get; private set; }

        public Viewport Viewport { get; private set; }

        [Preserve]
        public RedBallGame() : base(new ApplicationOptions(assetsFolder: "Data") { Height = 1024, Width = 576, Orientation = ApplicationOptions.OrientationType.Portrait }) { }

        [Preserve]
        public RedBallGame(ApplicationOptions opts) : base(opts) { }

        static RedBallGame()
        {
            UnhandledException += (s, e) =>
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                e.Handled = true;
            };
        }

        protected override void Start()
        {
            base.Start();
            CreateScene();
        }

        /// <summary>
        /// Adds the collision debug box.
        /// Implement the function below to the start for a Collision Debug Box
        ///Input.KeyDown+=(e =>
        ///    {
        ///        if (e.Key == Key.Esc) Exit();
        ///        if (e.Key == Key.C) AddCollisionDebugBox(scene, true);
        ///        if (e.Key == Key.V) AddCollisionDebugBox(scene, false);
        ///});
        /// </summary>
        /// <param name="">Root node.</param>
        /// <param name="">If set to <c>true</c> add.</param>
        /* Along with uncommenting the function
        static void AddCollisionDebugBox(Node rootNode, bool add)
        {
            var nodes = rootNode.GetChildrenWithComponent<CollisionShape>(true);
            foreach (var node in nodes)
            {
                node.GetChild("CollisionDebugBox", false)?.Remove();
                if (!add)
                    continue;
                var subNode = node.CreateChild("CollisionDebugBox");
                var box = subNode.CreateComponent<Box>();
                subNode.Scale = node.GetComponent<CollisionShape>().WorldBoundingBox.Size;
                box.Color = new Color(Color.Red, 0.4f);
            }
        }
        */

        async Task StartGame()
        {
            // Create the player
            Player = new Player();
            // add the aircraft component in Node form then add to the Octree of the scene
            var aircraftNode = scene.CreateChild(nameof(Aircraft));
            // Add the player as a component to the Node
            aircraftNode.AddComponent(Player);
            var playersLife = Player.Play();
            await playersLife;
            // After the playersLife is gone then remove the aircraft
            aircraftNode.Remove();
        }

        async void CreateScene()
        {
            scene = new Scene();
            scene.CreateComponent<Octree>();

            var physics = scene.CreateComponent<PhysicsWorld>();
            physics.SetGravity(new Vector3(0, 0, 0));

            // Camera
            var cameraNode = scene.CreateChild();
            cameraNode.Position = (new Vector3(0.0f, 0.0f, -10.0f));
            cameraNode.CreateComponent<Camera>();
            Viewport = new Viewport(Context, scene, cameraNode.GetComponent<Camera>(), null);

            if (Platform != Platforms.Android && Platform != Platforms.iOS)
            {
                RenderPath effectRenderPath = Viewport.RenderPath.Clone();
                var fxaaRp = ResourceCache.GetXmlFile(Assets.PostProcess.FXAA3);
                effectRenderPath.Append(fxaaRp);
                Viewport.RenderPath = effectRenderPath;
            }
            Renderer.SetViewport(0, Viewport);

            var zoneNode = scene.CreateChild();
            var zone = zoneNode.CreateComponent<Zone>();
            zone.SetBoundingBox(new BoundingBox(-300.0f, 300.0f));
            zone.AmbientColor = new Color(1f, 1f, 1f);
            
            // Background creation and addition to Octree
            var background = new Background();
            scene.AddComponent(background);
            // Initialize the Background
            background.Start();

            // Lights:
            var lightNode = scene.CreateChild();
            lightNode.Position = new Vector3(0, -5, -40);
            lightNode.AddComponent(new Light { Range = 120, Brightness = 0.8f });

            // Game logic cycle
            bool firstCycle = true;
            while (true)
            {
                var startMenu = scene.CreateComponent<StartMenu>();
                await startMenu.ShowStartMenu(!firstCycle); //wait for "start"
                startMenu.Remove();
                await StartGame();
                // Players death calls for first life cycle false
                firstCycle = false;
            }
        }
    }
}