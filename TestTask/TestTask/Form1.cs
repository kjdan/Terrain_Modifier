using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace TestTask
{
    public partial class Form1 : Form
    {

        private Device device = null;
        private VertexBuffer vb = null;
        private IndexBuffer ib = null;


        private int worldSize = 1;
        private int brushPower = 1;
        private int brushRange = 1;
        private int terWidth = 64;
        private int terLength = 64;
        private float raiseConst = 0.15f;
        private float lowerConst = -0.15f;

        private float moveSpeed = 2f;
        private float turnSpeed = 0.02f;
        private float rotY = 0; 
        private float tempY = 0;
        private float rotXZ = 0;
        private float tempXZ = 0;

        bool isMiddleMouseDown = false;
        bool isLeftMouseDown = false;
        bool isRightMouseDown = false;

        private Vector3 camPosition, camLookAt, camUp;


        CustomVertex.PositionColored[] verts = null; 

        private BrushDiffrentTypes mainBrushType = BrushDiffrentTypes.None;

        private static int[] indices = null;

        private FillMode fillMode = FillMode.WireFrame;

        private Color backgroundColor = Color.Black;

        private bool invalidating = true;

        private Bitmap heightmap = null;

        public int dataControl = 0;
        /// GRAPHIC FUNCTIONS
        public Form1()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true); 
            InitializeComponent();
            
            InitializeGraphics();
            InitializeEventHandler();
        }

        private void InitializeGraphics()
        {
            PresentParameters pp = new PresentParameters();
            pp.Windowed = true;
            pp.SwapEffect = SwapEffect.Discard; 
            pp.EnableAutoDepthStencil = true;
            pp.AutoDepthStencilFormat = DepthFormat.D16;

            device = new Device(0, DeviceType.Hardware, this, CreateFlags.HardwareVertexProcessing, pp);

            GenerateVertex();
            GenerateIndex();

            vb = new VertexBuffer(typeof(CustomVertex.PositionColored),  terWidth * terLength, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);
            OnVertexBufferCreate(vb, null);

            ib = new IndexBuffer(typeof(int), (terWidth - 1) * (terLength - 1) * 9, device, Usage.WriteOnly, Pool.Default);
            OnIndexBufferCreate(ib, null);


            camPosition = new Vector3(terLength / 2,104.5f, -3.5f);
            camUp = new Vector3(0, 1, 0); 
        }
        private void InitializeEventHandler()
        {
            vb.Created += new EventHandler(OnVertexBufferCreate);
            ib.Created += new EventHandler(OnIndexBufferCreate);

            this.KeyDown += new KeyEventHandler(OnKeyDown);
            this.MouseWheel += new MouseEventHandler(OnMouseScroll);

            this.MouseMove += new MouseEventHandler(OnMouseMove);
            this.MouseDown += new MouseEventHandler(OnMouseDown);
            this.MouseUp += new MouseEventHandler(OnMouseUp);

        }

        private void OnIndexBufferCreate(object sender, EventArgs e)
        {
            IndexBuffer buffer = (IndexBuffer)sender;
            buffer.SetData(indices, 0, LockFlags.None); 
        }
        private void OnVertexBufferCreate(object sender, EventArgs e)
        {
            VertexBuffer buffer = (VertexBuffer)sender;
            buffer.SetData(verts, 0, LockFlags.None); 
        }
        private void SetupCamera()
        {
            camLookAt.X = (float)Math.Sin(rotY) + camPosition.X + (float)(Math.Sin(rotXZ) * Math.Sin(rotY));       
            camLookAt.Y = (float)Math.Sin(rotXZ) + camPosition.Y;  
            camLookAt.Z = (float)Math.Cos(rotY) + camPosition.Z + (float)(Math.Sin(rotXZ) * Math.Cos(rotY));  


            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, this.Width / this.Height, 1.0f, 1000.0f);  
            device.Transform.View = Matrix.LookAtLH(camPosition, camLookAt, camUp); 

            device.RenderState.Lighting = false;
            device.RenderState.CullMode = Cull.CounterClockwise; 
            device.RenderState.FillMode = fillMode; 
        }

        private void GenerateVertex()
        {
            verts = new CustomVertex.PositionColored[ terWidth * terLength ];

            int k = 0;
            for (int z = 0; z < terWidth ; z++)
            {
                for (int x = 0; x < terLength ; x++)
                {
                    verts[k].Position = new Vector3(x, 100, z);
                    verts[k].Color = Color.White.ToArgb();
                    k++;
                }
            }

        }
        private void GenerateIndex() 
        {
            indices = new int[(terWidth - 1) * (terLength - 1) * 9 ];

            int k = 0;
            int l = 0;
            for (int i = 0; i < (terWidth - 1) * (terLength - 1) * 9 ; i += 9)
            {
                indices[i] = k;                     
                indices[i + 1] = k + terLength;     
                indices[i + 2] = k + terLength + 1; 

                indices[i + 3] = k;                   
                indices[i + 4] = k + terLength + 1;   
                indices[i + 5] = k + 1;

                indices[i + 6] = k;
                indices[i + 7] = k + terLength;
                indices[i + 8] = k + 1;


                k++;
                l++;
                if (l == terLength - 1) 
                {
                    l = 0;
                    k++;
                }
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {


            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, backgroundColor, 1, 0);

            SetupCamera();

            device.BeginScene();

            device.VertexFormat = CustomVertex.PositionColored.Format;

            device.SetStreamSource(0, vb, 0);
            device.Indices = ib;

            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, terWidth * terLength, 0, (terWidth - 1) * (terLength - 1) * 9 / 3);

            device.EndScene();
            device.Present();
            menuStrip1.Update();

            if (invalidating)
            {
                this.Invalidate();
            }
        }




        ///CONTROL FUNCTIONS
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case (Keys.W):
                    {
                        camPosition.X += moveSpeed * (float)Math.Sin(rotY);
                        camPosition.Z += moveSpeed * (float)Math.Cos(rotY);
                        break;
                    }
                case (Keys.S):
                    {
                        camPosition.X -= moveSpeed * (float)Math.Sin(rotY);
                        camPosition.Z -= moveSpeed * (float)Math.Cos(rotY);
                        break;
                    }
                case (Keys.D):
                    {
                        camPosition.X += moveSpeed * (float)Math.Sin(rotY + Math.PI / 2);
                        camPosition.Z += moveSpeed * (float)Math.Cos(rotY + Math.PI / 2);

                        break;
                    }

                case (Keys.A):
                    {
                        camPosition.X -= moveSpeed * (float)Math.Sin(rotY + Math.PI / 2);
                        camPosition.Z -= moveSpeed * (float)Math.Cos(rotY + Math.PI / 2);
                        break;
                    }
              
            }
        }
        private void OnMouseScroll(object sender, MouseEventArgs e)
        {
            camPosition.Y -= e.Delta * 0.1f;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isMiddleMouseDown)
            {
                rotY = tempY + e.X * turnSpeed;

                float tmp = tempXZ - e.Y * turnSpeed / 4;
                if (tmp < Math.PI / 2 && tmp > -Math.PI / 2)
                {
                    rotXZ = tmp;
                }

            }
            if (isLeftMouseDown)
            {
                Point mouseMoveLocation = new Point(e.X, e.Y);
                TerrainUp(mouseMoveLocation);
            }
            if (isRightMouseDown)
            {
                Point mouseMoveLocation = new Point(e.X, e.Y);
                TerrainDown(mouseMoveLocation);
            }

        }


        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case (MouseButtons.Middle):
                    {
                        tempY = rotY - e.X * turnSpeed;
                        tempXZ = rotXZ + e.Y * turnSpeed / 4;

                        isMiddleMouseDown = true;
                        break;
                    }
                case (MouseButtons.Left):
                    {
                        isLeftMouseDown = true;

                        Point mouseDownLocation = new Point(e.X, e.Y);
                        TerrainUp(mouseDownLocation);
                        break;
                    }

                case (MouseButtons.Right):
                    {
                        isRightMouseDown = true;

                        Point mouseDownLocation = new Point(e.X, e.Y);
                        TerrainDown(mouseDownLocation);
                        break;
                    }
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case (MouseButtons.Middle):
                    {
                        isMiddleMouseDown = false;
                        break;
                    }
                case (MouseButtons.Left):
                    {
                        isLeftMouseDown = false;
                        break;
                    }
                case (MouseButtons.Right):
                    {
                        isRightMouseDown = false;
                        break;
                    }
            }
        }

     

       

   
        ///
        private void TerrainUp(Point mouseLocation)
        {

            if (mainBrushType != BrushDiffrentTypes.None)
            {
                IntersectInformation hitLocation;

                Vector3 near, far, direction;

                near = new Vector3(mouseLocation.X, mouseLocation.Y, 0);
                far = new Vector3(mouseLocation.X, mouseLocation.Y, 100);

                near.Unproject(device.Viewport, device.Transform.Projection, device.Transform.View, device.Transform.World);
                far.Unproject(device.Viewport, device.Transform.Projection, device.Transform.View, device.Transform.World);

                direction = near - far;

                for (int i = 0; i < (terWidth - 1) * (terLength - 1) * 9; i += 3)
                {
                    ///check
                    if (Geometry.IntersectTri(verts[indices[i]].Position, verts[indices[i + 1]].Position, verts[indices[i + 2]].Position, near, direction, out hitLocation))
                    {


                        if (mainBrushType == BrushDiffrentTypes.Triangle)
                        {
                            if (indices[i + 2] - indices[i + 1] == 1)
                            {
                                if (brushRange >= 1)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i + 2)
                                    {
                                        verts[indices[i]].Color = Color.Red.ToArgb();
                                        verts[indices[i + 1]].Color = Color.Red.ToArgb();
                                        verts[indices[i + 2]].Color = Color.Red.ToArgb();

                                        verts[indices[i]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + 1]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + 2]].Position += new Vector3(0, raiseConst, 0);
                                    }


                                }
                                if (brushRange >= 2)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i + (terLength * 9) + 2)
                                    {
                                        verts[indices[i + (terLength * 9) + 1]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) - 2]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) + 2]].Color = Color.Red.ToArgb();


                                        verts[indices[i + (terLength * 9) + 1]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 9) - 2]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 9) + 2]].Position += new Vector3(0, raiseConst, 0);
                                    }

                                }
                                if (brushRange >= 3)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i + (terLength * 2 * 9) + 2)
                                    {
                                        verts[indices[i + (terLength * 2 * 9) - 11]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) - 2]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) + 1]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) + 2]].Color = Color.Red.ToArgb();


                                        verts[indices[i + (terLength * 2 * 9) - 11]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) - 2]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) + 1]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) + 2]].Position += new Vector3(0, raiseConst, 0);
                                    }
                                }
                            }
                            else
                            {
                                if (brushRange >= 1)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i - 1 && i-3>=0)
                                    {
                                        verts[indices[i - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i -2]].Color = Color.Red.ToArgb();
                                        verts[indices[i -1]].Color = Color.Red.ToArgb();

                                        verts[indices[i - 3]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i -2]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i- 1]].Position += new Vector3(0, raiseConst, 0);
                                    }


                                }
                                if (brushRange >= 2)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i + (terLength * 9) - 1)
                                    {
                                        verts[indices[i + (terLength * 9) -2]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) - 5]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) -1]].Color = Color.Red.ToArgb();


                                        verts[indices[i + (terLength * 9) -2]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 9) - 5]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 9) -1]].Position += new Vector3(0, raiseConst, 0);
                                    }

                                }
                                if (brushRange >= 3)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i + (terLength * 2 * 9) - 1)
                                    {
                                        verts[indices[i + (terLength * 2 * 9) - 14]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) - 5]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) -2]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) -1]].Color = Color.Red.ToArgb();


                                        verts[indices[i + (terLength * 2 * 9) -14]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) - 5]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) -2]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) -1]].Position += new Vector3(0, raiseConst, 0);
                                    }
                                }
                            }
                                  


                        }
                        else if (mainBrushType == BrushDiffrentTypes.Square)
                        {

                            if (indices[i + 2] - indices[i + 1] == 1)
                            {
                                if (brushRange >= 1)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i + 5)
                                    {
                                        verts[indices[i]].Color = Color.Red.ToArgb();
                                        verts[indices[i + 1]].Color = Color.Red.ToArgb();
                                        verts[indices[i + 2]].Color = Color.Red.ToArgb();
                                        verts[indices[i + 5]].Color = Color.Red.ToArgb();

                                        verts[indices[i]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + 1]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + 2]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + 5]].Position += new Vector3(0, raiseConst, 0);
                                    }
                                }
                                if (brushRange >= 2)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i + (terLength * 9) + 5)
                                    {
                                        verts[indices[i + 14]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) + 1]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) + 2]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) - 2]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) + 5]].Color = Color.Red.ToArgb();

                                        verts[indices[i + 14]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 9) + 1]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 9) + 2]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 9) - 2]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 9) + 5]].Position += new Vector3(0, raiseConst, 0);
                                    }
                                }
                                if (brushRange >= 3)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i + (terLength * 2 * 9) + 5 && (terWidth - 1) * (terLength - 1) * 9 > i + (terLength * 9) + 24)
                                    {
                                        verts[indices[i + 23]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) + 24]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) + 1]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) + 2]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) + 5]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) - 2]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) - 11]].Color = Color.Red.ToArgb();

                                        verts[indices[i + 23]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 9) + 24]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) + 1]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) + 2]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) + 5]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) - 2]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) - 11]].Position += new Vector3(0, raiseConst, 0);
                                    }
                                }
                            }
                            else
                            {
                                if (brushRange >= 1)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i + 2 && i-3>=0)
                                    {
                                        verts[indices[i - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i - 2]].Color = Color.Red.ToArgb();
                                        verts[indices[i - 1]].Color = Color.Red.ToArgb();
                                        verts[indices[i + 2]].Color = Color.Red.ToArgb();

                                        verts[indices[i - 3]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i - 2]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i - 1]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + 2]].Position += new Vector3(0, raiseConst, 0);
                                    }
                                }
                                if (brushRange >= 2)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i + (terLength * 9) + 2)
                                    {
                                        verts[indices[i + 11]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) - 2]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) - 1]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) - 5]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) + 2]].Color = Color.Red.ToArgb();

                                        verts[indices[i + 14 - 3]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 9) - 2]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 9) - 1]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 9) - 5]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 9) + 2]].Position += new Vector3(0, raiseConst, 0);
                                    }
                                }
                                if (brushRange >= 3)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i + (terLength * 2 * 9) + 2 && (terWidth - 1) * (terLength - 1) * 9 > i + (terLength * 9) + 21)
                                    {
                                        verts[indices[i + 23 - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) + 21]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) - 2]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) - 1]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) + 2]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) - 5]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) - 14]].Color = Color.Red.ToArgb();

                                        verts[indices[i + 23 - 3]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 9) + 21]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) - 2]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) - 1]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) + 2]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) - 5]].Position += new Vector3(0, raiseConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) - 14]].Position += new Vector3(0, raiseConst, 0);
                                    }
                                }
                            }
                        }
                        else if (mainBrushType == BrushDiffrentTypes.Cricle)
                        {


                            if(brushRange >= 1)
                            {
                                if ((terWidth - 1) * (terLength - 1) * 9 > i + 18 + (terLength * 9) && (terWidth - 1) * (terLength - 1) * 9 > i +  (terLength * 2 * 9)  && i + 18 - (terLength * 9) >= 0)
                                {
                                    verts[indices[i + 0]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 9]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 18]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 27]].Color = Color.Red.ToArgb();
                                    verts[indices[i - 9 + (terLength * 9)]].Color = Color.Red.ToArgb();
                                    verts[indices[i + (terLength * 9) ]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 9 + (terLength * 9)]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 18 + (terLength * 9)]].Color = Color.Red.ToArgb();
                                    verts[indices[i - 9 + (terLength * 2 * 9)]].Color = Color.Red.ToArgb();
                                    verts[indices[i + (terLength * 2 * 9)]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 18 - (terLength * 9)]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 27 - (terLength * 9)]].Color = Color.Red.ToArgb();

                                    verts[indices[i + 0]].Position += new Vector3(0, raiseConst, 0);
                                    verts[indices[i + 9]].Position += new Vector3(0, raiseConst, 0);
                                    verts[indices[i + 18]].Position += new Vector3(0, raiseConst, 0);
                                    verts[indices[i + 27]].Position += new Vector3(0, raiseConst, 0);
                                    verts[indices[i - 9 + (terLength * 9)]].Position += new Vector3(0, raiseConst, 0);
                                    verts[indices[i +  (terLength * 9)]].Position += new Vector3(0, raiseConst, 0);
                                    verts[indices[i + 9 + (terLength * 9)]].Position += new Vector3(0, raiseConst, 0);
                                    verts[indices[i + 18 + (terLength * 9)]].Position += new Vector3(0, raiseConst, 0);
                                    verts[indices[i - 9 + (terLength * 2 * 9)]].Position += new Vector3(0, raiseConst, 0);
                                    verts[indices[i + (terLength * 2 * 9)]].Position += new Vector3(0, raiseConst, 0);
                                    verts[indices[i + 18 - (terLength * 9)]].Position += new Vector3(0, raiseConst, 0);
                                    verts[indices[i + 27 - (terLength * 9)]].Position += new Vector3(0, raiseConst, 0);
                                }

                            }
                            if (brushRange >= 2)
                            {
                                if ((terWidth - 1) * (terLength - 1) * 9 > i + (terLength * 2 * 9) - 18 && i-9>=0 && i - (terLength * 9) + 9 >=0)
                                {
                                    verts[indices[i - 9]].Color = Color.Red.ToArgb();
                                    verts[indices[i - 18 + (terLength * 9)]].Color = Color.Red.ToArgb();
                                    verts[indices[i -18 + (terLength * 2 * 9)]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 9 - (terLength * 9)]].Color = Color.Red.ToArgb();

                                    verts[indices[i - 9]].Position += new Vector3(0, raiseConst, 0);
                                    verts[indices[i - 18 + (terLength * 9)]].Position += new Vector3(0, raiseConst, 0);
                                    verts[indices[i - 18 + (terLength * 2 * 9)]].Position += new Vector3(0, raiseConst, 0);
                                    verts[indices[i + 9 - (terLength * 9)]].Position += new Vector3(0, raiseConst, 0);
                                }
                            }
                            if (brushRange >= 3)
                            {
                                if ((terWidth - 1) * (terLength - 1) * 9 > i + 18 + (terLength * 3 * 9) - 27 && i - 9 + (terLength * 2 * 9) - 18>=0)
                                {
                                    verts[indices[i - 27 + (terLength * 2 * 9)]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 9 + (terLength * 2 * 9)]].Color = Color.Red.ToArgb();
                                    verts[indices[i - 27 + (terLength * 3 * 9)]].Color = Color.Red.ToArgb();
                                    verts[indices[i -18 + (terLength * 3 * 9)]].Color = Color.Red.ToArgb();
                                    verts[indices[i - 9 + (terLength * 3 * 9)]].Color = Color.Red.ToArgb();

                                    verts[indices[i - 27 + (terLength * 2 * 9)]].Position += new Vector3(0, raiseConst, 0);
                                    verts[indices[i + 9 + (terLength * 2 * 9)]].Position += new Vector3(0, raiseConst, 0);
                                    verts[indices[i - 27 + (terLength * 3 * 9)]].Position += new Vector3(0, raiseConst, 0);
                                    verts[indices[i - 18 + (terLength * 3 * 9)]].Position += new Vector3(0, raiseConst, 0);
                                    verts[indices[i - 9 + (terLength * 3 * 9)]].Position += new Vector3(0, raiseConst, 0);
                                }
                            }
                            
                        }




                        vb.SetData(verts, 0, LockFlags.None);
                        break;
                    }
                    
                }
            }
        }
        private void TerrainDown(Point mouseLocation)
        {

            if (mainBrushType != BrushDiffrentTypes.None)
            {
                IntersectInformation hitLocation;

                Vector3 near, far, direction;

                near = new Vector3(mouseLocation.X, mouseLocation.Y, 0);
                far = new Vector3(mouseLocation.X, mouseLocation.Y, 100);

                near.Unproject(device.Viewport, device.Transform.Projection, device.Transform.View, device.Transform.World);
                far.Unproject(device.Viewport, device.Transform.Projection, device.Transform.View, device.Transform.World);

                direction = near - far;

                for (int i = 0; i < (terWidth - 1) * (terLength - 1) * 9; i += 3)
                {
                    ///check
                    if (Geometry.IntersectTri(verts[indices[i]].Position, verts[indices[i + 1]].Position, verts[indices[i + 2]].Position, near, direction, out hitLocation))
                    {


                        if (mainBrushType == BrushDiffrentTypes.Triangle)
                        {
                            if (indices[i + 2] - indices[i + 1] == 1)
                            {
                                if (brushRange >= 1)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i + 2)
                                    {
                                        verts[indices[i]].Color = Color.Red.ToArgb();
                                        verts[indices[i + 1]].Color = Color.Red.ToArgb();
                                        verts[indices[i + 2]].Color = Color.Red.ToArgb();

                                        verts[indices[i]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + 1]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + 2]].Position += new Vector3(0, lowerConst, 0);
                                    }


                                }
                                if (brushRange >= 2)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i + (terLength * 9) + 2)
                                    {
                                        verts[indices[i + (terLength * 9) + 1]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) - 2]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) + 2]].Color = Color.Red.ToArgb();


                                        verts[indices[i + (terLength * 9) + 1]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 9) - 2]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 9) + 2]].Position += new Vector3(0, lowerConst, 0);
                                    }

                                }
                                if (brushRange >= 3)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i + (terLength * 2 * 9) + 2)
                                    {
                                        verts[indices[i + (terLength * 2 * 9) - 11]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) - 2]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) + 1]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) + 2]].Color = Color.Red.ToArgb();


                                        verts[indices[i + (terLength * 2 * 9) - 11]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) - 2]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) + 1]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) + 2]].Position += new Vector3(0, lowerConst, 0);
                                    }
                                }
                            }
                            else
                            {
                                if (brushRange >= 1)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i - 1 && i - 3 >= 0)
                                    {
                                        verts[indices[i - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i + 1 - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i + 2 - 3]].Color = Color.Red.ToArgb();

                                        verts[indices[i - 3]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + 1 - 3]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + 2 - 3]].Position += new Vector3(0, lowerConst, 0);
                                    }


                                }
                                if (brushRange >= 2)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i + (terLength * 9) - 1)
                                    {
                                        verts[indices[i + (terLength * 9) + 1 - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) - 2 - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) + 2 - 3]].Color = Color.Red.ToArgb();


                                        verts[indices[i + (terLength * 9) + 1 - 3]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 9) - 2 - 3]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 9) + 2 - 3]].Position += new Vector3(0, lowerConst, 0);
                                    }

                                }
                                if (brushRange >= 3)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i + (terLength * 2 * 9) - 1)
                                    {
                                        verts[indices[i + (terLength * 2 * 9) - 11 - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) - 2 - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) + 1 - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) + 2 - 3]].Color = Color.Red.ToArgb();


                                        verts[indices[i + (terLength * 2 * 9) - 11 - 3]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) - 2 - 3]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) + 1 - 3]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) + 2 - 3]].Position += new Vector3(0, lowerConst, 0);
                                    }
                                }
                            }



                        }
                        else if (mainBrushType == BrushDiffrentTypes.Square)
                        {

                            if (indices[i + 2] - indices[i + 1] == 1)
                            {
                                if (brushRange >= 1)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i + 5)
                                    {
                                        verts[indices[i]].Color = Color.Red.ToArgb();
                                        verts[indices[i + 1]].Color = Color.Red.ToArgb();
                                        verts[indices[i + 2]].Color = Color.Red.ToArgb();
                                        verts[indices[i + 5]].Color = Color.Red.ToArgb();

                                        verts[indices[i]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + 1]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + 2]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + 5]].Position += new Vector3(0, lowerConst, 0);
                                    }
                                }
                                if (brushRange >= 2)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i + (terLength * 9) + 5)
                                    {
                                        verts[indices[i + 14]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) + 1]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) + 2]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) - 2]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) + 5]].Color = Color.Red.ToArgb();

                                        verts[indices[i + 14]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 9) + 1]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 9) + 2]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 9) - 2]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 9) + 5]].Position += new Vector3(0, lowerConst, 0);
                                    }
                                }
                                if (brushRange >= 3)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i + (terLength * 2 * 9) + 5 && (terWidth - 1) * (terLength - 1) * 9 > i + (terLength * 9) + 24)
                                    {
                                        verts[indices[i + 23]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) + 24]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) + 1]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) + 2]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) + 5]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) - 2]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) - 11]].Color = Color.Red.ToArgb();

                                        verts[indices[i + 23]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 9) + 24]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) + 1]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) + 2]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) + 5]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) - 2]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) - 11]].Position += new Vector3(0, lowerConst, 0);
                                    }
                                }
                            }
                            else
                            {
                                if (brushRange >= 1)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i + 5 - 3 && i - 3 >= 0)
                                    {
                                        verts[indices[i - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i + 1 - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i + 2 - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i + 5 - 3]].Color = Color.Red.ToArgb();

                                        verts[indices[i - 3]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + 1 - 3]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + 2 - 3]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + 5 - 3]].Position += new Vector3(0, lowerConst, 0);
                                    }
                                }
                                if (brushRange >= 2)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i + (terLength * 9) + 5 - 3)
                                    {
                                        verts[indices[i + 14 - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) + 1 - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) + 2 - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) - 2 - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) + 5 - 3]].Color = Color.Red.ToArgb();

                                        verts[indices[i + 14 - 3]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 9) + 1 - 3]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 9) + 2 - 3]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 9) - 2 - 3]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 9) + 5 - 3]].Position += new Vector3(0, lowerConst, 0);
                                    }
                                }
                                if (brushRange >= 3)
                                {
                                    if ((terWidth - 1) * (terLength - 1) * 9 > i + (terLength * 2 * 9) + 5 - 3 && (terWidth - 1) * (terLength - 1) * 9 > i + (terLength * 9) + 24 - 3)
                                    {
                                        verts[indices[i + 23 - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 9) + 24 - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) + 1 - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) + 2 - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) + 5 - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) - 2 - 3]].Color = Color.Red.ToArgb();
                                        verts[indices[i + (terLength * 2 * 9) - 11 - 3]].Color = Color.Red.ToArgb();

                                        verts[indices[i + 23 - 3]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 9) + 24 - 3]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) + 1 - 3]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) + 2 - 3]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) + 5 - 3]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) - 2 - 3]].Position += new Vector3(0, lowerConst, 0);
                                        verts[indices[i + (terLength * 2 * 9) - 11 - 3]].Position += new Vector3(0, lowerConst, 0);
                                    }
                                }
                            }
                        }
                        else if (mainBrushType == BrushDiffrentTypes.Cricle)
                        {


                            if (brushRange >= 1)
                            {
                                if ((terWidth - 1) * (terLength - 1) * 9 > i + 27 + (terLength * 9) - 9 && (terWidth - 1) * (terLength - 1) * 9 > i + 18 + (terLength * 2 * 9) - 18 && i + 9 - (terLength * 9) + 9 >= 0)
                                {
                                    verts[indices[i + 0]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 9]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 18]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 27]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 0 + (terLength * 9) - 9]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 9 + (terLength * 9) - 9]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 18 + (terLength * 9) - 9]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 27 + (terLength * 9) - 9]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 9 + (terLength * 2 * 9) - 18]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 18 + (terLength * 2 * 9) - 18]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 9 - (terLength * 9) + 9]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 18 - (terLength * 9) + 9]].Color = Color.Red.ToArgb();

                                    verts[indices[i + 0]].Position += new Vector3(0, lowerConst, 0);
                                    verts[indices[i + 9]].Position += new Vector3(0, lowerConst, 0);
                                    verts[indices[i + 18]].Position += new Vector3(0, lowerConst, 0);
                                    verts[indices[i + 27]].Position += new Vector3(0, lowerConst, 0);
                                    verts[indices[i + 0 + (terLength * 9) - 9]].Position += new Vector3(0, lowerConst, 0);
                                    verts[indices[i + 9 + (terLength * 9) - 9]].Position += new Vector3(0, lowerConst, 0);
                                    verts[indices[i + 18 + (terLength * 9) - 9]].Position += new Vector3(0, lowerConst, 0);
                                    verts[indices[i + 27 + (terLength * 9) - 9]].Position += new Vector3(0, lowerConst, 0);
                                    verts[indices[i + 9 + (terLength * 2 * 9) - 18]].Position += new Vector3(0, lowerConst, 0);
                                    verts[indices[i + 18 + (terLength * 2 * 9) - 18]].Position += new Vector3(0, lowerConst, 0);
                                    verts[indices[i + 9 - (terLength * 9) + 9]].Position += new Vector3(0, lowerConst, 0);
                                    verts[indices[i + 18 - (terLength * 9) + 9]].Position += new Vector3(0, lowerConst, 0);
                                }

                            }
                            if (brushRange >= 2)
                            {
                                if ((terWidth - 1) * (terLength - 1) * 9 > i + (terLength * 2 * 9) - 18 && i - 9 >= 0 && i - (terLength * 9) + 9 >= 0)
                                {
                                    verts[indices[i - 9]].Color = Color.Red.ToArgb();
                                    verts[indices[i - 9 + (terLength * 9) - 9]].Color = Color.Red.ToArgb();
                                    verts[indices[i + (terLength * 2 * 9) - 18]].Color = Color.Red.ToArgb();
                                    verts[indices[i - (terLength * 9) + 9]].Color = Color.Red.ToArgb();

                                    verts[indices[i - 9]].Position += new Vector3(0, lowerConst, 0);
                                    verts[indices[i - 9 + (terLength * 9) - 9]].Position += new Vector3(0, lowerConst, 0);
                                    verts[indices[i + (terLength * 2 * 9) - 18]].Position += new Vector3(0, lowerConst, 0);
                                    verts[indices[i - (terLength * 9) + 9]].Position += new Vector3(0, lowerConst, 0);
                                }
                            }
                            if (brushRange >= 3)
                            {
                                if ((terWidth - 1) * (terLength - 1) * 9 > i + 18 + (terLength * 3 * 9) - 27 && i - 9 + (terLength * 2 * 9) - 18 >= 0)
                                {
                                    verts[indices[i - 9 + (terLength * 2 * 9) - 18]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 27 + (terLength * 2 * 9) - 18]].Color = Color.Red.ToArgb();
                                    verts[indices[i + (terLength * 3 * 9) - 27]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 9 + (terLength * 3 * 9) - 27]].Color = Color.Red.ToArgb();
                                    verts[indices[i + 18 + (terLength * 3 * 9) - 27]].Color = Color.Red.ToArgb();

                                    verts[indices[i - 9 + (terLength * 2 * 9) - 18]].Position += new Vector3(0, lowerConst, 0);
                                    verts[indices[i + 27 + (terLength * 2 * 9) - 18]].Position += new Vector3(0, lowerConst, 0);
                                    verts[indices[i + (terLength * 3 * 9) - 27]].Position += new Vector3(0, lowerConst, 0);
                                    verts[indices[i + 9 + (terLength * 3 * 9) - 27]].Position += new Vector3(0, lowerConst, 0);
                                    verts[indices[i + 18 + (terLength * 3 * 9) - 27]].Position += new Vector3(0, lowerConst, 0);
                                }
                            }

                        }




                        vb.SetData(verts, 0, LockFlags.None);
                        break;
                    }

                }
            }
        }
        ///Load function
        private void LoadHeightmap()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {

                ofd.Title = "Load Heightmap";
                ofd.Filter = "Bitmap files (*.png)|*.png";
                ofd.InitialDirectory = Application.StartupPath;
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    heightmap = new Bitmap(ofd.FileName);
                    Color pixelCol;
                    verts = new CustomVertex.PositionColored[ terWidth * terLength];
                    int k = 0;
                    for (int z = 0; z < terWidth; z++)
                    {
                        for (int x = 0; x < terLength; x++)
                        {
                            if (heightmap.Size.Width > x && heightmap.Size.Height > z)
                            {
                                
                                pixelCol = heightmap.GetPixel(x, z);
                                verts[k].Position = new Vector3(x, (float)pixelCol.A, z);
                                verts[k].Color = pixelCol.ToArgb();
                            }
                            else
                            {
                                verts[k].Position = new Vector3(x, 100, z);
                                verts[k].Color = Color.White.ToArgb();
                            }

                            k++;
                        }
                    }
                   
                }
            }
        }
        ///Save function
        private void SaveHeightmap()
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {

                    Bitmap heightmap = new Bitmap(terWidth, terLength);
                    int k = 0;
                    for (int z = 0; z < terWidth; z++)
                    {
                        for (int x = 0; x < terLength; x++)
                        {
                            Color check = Color.FromArgb(verts[k].Color);
                            Color myRgbColor = new Color();
                            myRgbColor = Color.FromArgb((int)verts[k].Position.Y, check.R, check.G, check.B);
                            heightmap.SetPixel(x, z, myRgbColor);

                            k++;
                        }
                    }

                    heightmap.Save("testOutput" + dataControl + ".png");
                    dataControl++;

                }
            }
        }




        ///BUTTONS FUNCTIONS
        ///
        ///Brush types buttons functions
        private void squareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mainBrushType = BrushDiffrentTypes.Square;
        }

        private void noneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mainBrushType = BrushDiffrentTypes.None;
        }

        private void circleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mainBrushType = BrushDiffrentTypes.Cricle;
        }

        private void triangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mainBrushType = BrushDiffrentTypes.Triangle;
        }


        ///Brush size buttons functions
        private void smallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            brushRange = 1;
        }

        private void mediumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            brushRange = 2;
        }

        private void largeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            brushRange = 3;
        }
        ///World size buttons functions 
        private void x64ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            terLength = 64;
            terWidth = 64;
            GenerateVertex();
            GenerateIndex();

            vb = new VertexBuffer(typeof(CustomVertex.PositionColored),  terWidth * terLength, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);
            OnVertexBufferCreate(vb, null);

            ib = new IndexBuffer(typeof(int), (terWidth - 1) * (terLength - 1) * 9, device, Usage.WriteOnly, Pool.Default);
            OnIndexBufferCreate(ib, null);
        }

        private void x128ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            terLength = 128;
            terWidth = 128;
            GenerateVertex();
            GenerateIndex();

            vb = new VertexBuffer(typeof(CustomVertex.PositionColored),  terWidth * terLength, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);
            OnVertexBufferCreate(vb, null);

            ib = new IndexBuffer(typeof(int), (terWidth - 1) * (terLength - 1) * 9, device, Usage.WriteOnly, Pool.Default);
            OnIndexBufferCreate(ib, null);
        }

        private void x256ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            terLength = 256;
            terWidth = 256;
            GenerateVertex();
            GenerateIndex();

            vb = new VertexBuffer(typeof(CustomVertex.PositionColored),  terWidth * terLength, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);
            OnVertexBufferCreate(vb, null);

            ib = new IndexBuffer(typeof(int), (terWidth - 1) * (terLength - 1) * 9, device, Usage.WriteOnly, Pool.Default);
            OnIndexBufferCreate(ib, null);
        }
        ///Strength buttons functions
        private void weakToolStripMenuItem_Click(object sender, EventArgs e)
        {
            raiseConst = 0.1f;
            lowerConst = -0.1f;
        }

        private void averageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            raiseConst = 0.3f;
            lowerConst = -0.3f;
        }

        private void strongToolStripMenuItem_Click(object sender, EventArgs e)
        {
            raiseConst = 0.4f;
            lowerConst = -0.4f;
        }


        ///Load and Save buttons functions

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveHeightmap();
           // vb.SetData(verts, 0, LockFlags.None);
        }

     
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadHeightmap();
            vb.SetData(verts, 0, LockFlags.None);
        }

    }

    
}
///Types of brushes
public enum BrushDiffrentTypes
{
    None,
    Square,
    Cricle,
    Triangle
}