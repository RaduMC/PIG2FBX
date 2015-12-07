using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PIG2FBX
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            string input = "";
            int totalConverted = 0;

            switch (args.Length)
            {
                case 0:
                    input = AppDomain.CurrentDomain.BaseDirectory;
                    break;
                case 1:
                    input = args[0];
                    break;
                default:
                    Console.WriteLine("PIG to FBX model converter\nby Chipicao - www.KotsChopShop.com\n\nUsage: PIG2FBX.exe [input_file/folder]");
                    break;
            }

            if (File.Exists(input))
            {
                input = Path.GetFullPath(input);
                ConvertPIG(input);
                Console.WriteLine("Finished converting " + Path.GetFileName(input));
            }
            else if (Directory.Exists(input))
            {
                string[] inputFiles = Directory.GetFiles(input, "*.pig");
                foreach (var inputFile in inputFiles)
                {
                    totalConverted += ConvertPIG(inputFile);
                }
                Console.WriteLine("Finished converting {0} files.", totalConverted);
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            else { Console.WriteLine("Invalid input file/folder: " + input); }
        }

        private static int ConvertPIG(string PIGfile)
        {
            int converted = 0;
            string output = Path.GetDirectoryName(PIGfile) + "\\" + Path.GetFileNameWithoutExtension(PIGfile) + ".fbx";

            if (File.Exists(output))
            {
                Console.WriteLine("File already exists: " + Path.GetFileNameWithoutExtension(PIGfile) + ".fbx");
            }
            else
            {
                Console.WriteLine("Converting " + Path.GetFileName(PIGfile));
                var pmodel = new PIGmodel(PIGfile);
                var timestamp = DateTime.Now;

                using (TextWriter sw = new StreamWriter(output))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("; FBX 7.1.0 project file");
                    sb.Append("\nFBXHeaderExtension:  {\n\tFBXHeaderVersion: 1003\n\tFBXVersion: 7100\n\tCreationTimeStamp:  {\n\t\tVersion: 1000");
                    sb.Append("\n\t\tYear: " + timestamp.Year);
                    sb.Append("\n\t\tMonth: " + timestamp.Month);
                    sb.Append("\n\t\tDay: " + timestamp.Day);
                    sb.Append("\n\t\tHour: " + timestamp.Hour);
                    sb.Append("\n\t\tMinute: " + timestamp.Minute);
                    sb.Append("\n\t\tSecond: " + timestamp.Second);
                    sb.Append("\n\t\tMillisecond: " + timestamp.Millisecond);
                    sb.Append("\n\t}\n\tCreator: \"PIG2FBX by Chipicao\"\n}\n");

                    sb.Append("\nGlobalSettings:  {");
                    sb.Append("\n\tVersion: 1000");
                    sb.Append("\n\tProperties70:  {");
                    sb.Append("\n\t\tP: \"UpAxis\", \"int\", \"Integer\", \"\",2");
                    sb.Append("\n\t\tP: \"UpAxisSign\", \"int\", \"Integer\", \"\",1");
                    sb.Append("\n\t\tP: \"FrontAxis\", \"int\", \"Integer\", \"\",1");
                    sb.Append("\n\t\tP: \"FrontAxisSign\", \"int\", \"Integer\", \"\",-1");
                    sb.Append("\n\t\tP: \"CoordAxis\", \"int\", \"Integer\", \"\",0");
                    sb.Append("\n\t\tP: \"CoordAxisSign\", \"int\", \"Integer\", \"\",1");
                    sb.Append("\n\t\tP: \"OriginalUpAxis\", \"int\", \"Integer\", \"\",2");
                    sb.Append("\n\t\tP: \"OriginalUpAxisSign\", \"int\", \"Integer\", \"\",1");
                    sb.Append("\n\t\tP: \"UnitScaleFactor\", \"double\", \"Number\", \"\",1");
                    sb.Append("\n\t\tP: \"OriginalUnitScaleFactor\", \"double\", \"Number\", \"\",1");
                    //sb.Append("\n\t\tP: \"AmbientColor\", \"ColorRGB\", \"Color\", \"\",0,0,0");
                    //sb.Append("\n\t\tP: \"DefaultCamera\", \"KString\", \"\", \"\", \"Producer Perspective\"");
                    //sb.Append("\n\t\tP: \"TimeMode\", \"enum\", \"\", \"\",6");
                    //sb.Append("\n\t\tP: \"TimeProtocol\", \"enum\", \"\", \"\",2");
                    //sb.Append("\n\t\tP: \"SnapOnFrameMode\", \"enum\", \"\", \"\",0");
                    //sb.Append("\n\t\tP: \"TimeSpanStart\", \"KTime\", \"Time\", \"\",0");
                    //sb.Append("\n\t\tP: \"TimeSpanStop\", \"KTime\", \"Time\", \"\",153953860000");
                    //sb.Append("\n\t\tP: \"CustomFrameRate\", \"double\", \"Number\", \"\",-1");
                    //sb.Append("\n\t\tP: \"TimeMarker\", \"Compound\", \"\", \"\"");
                    //sb.Append("\n\t\tP: \"CurrentTimeMarker\", \"int\", \"Integer\", \"\",-1");
                    sb.Append("\n\t}\n}\n");

                    sb.Append("\nDocuments:  {");
                    sb.Append("\n\tCount: 1");
                    sb.Append("\n\tDocument: 1234567890, \"\", \"Scene\" {");
                    sb.Append("\n\t\tProperties70:  {");
                    sb.Append("\n\t\t\tP: \"SourceObject\", \"object\", \"\", \"\"");
                    sb.Append("\n\t\t\tP: \"ActiveAnimStackName\", \"KString\", \"\", \"\", \"\"");
                    sb.Append("\n\t\t}");
                    sb.Append("\n\t\tRootNode: 0");
                    sb.Append("\n\t}\n}\n");
                    sb.Append("\nReferences:  {\n}\n");

                    sb.Append("\nDefinitions:  {");
                    sb.Append("\n\tVersion: 100");

                    sb.Append("\n\tObjectType: \"GlobalSettings\" {");
                    sb.Append("\n\t\tCount: 1");
                    sb.Append("\n\t}");

                    sb.Append("\n\tObjectType: \"Model\" {");
                    sb.Append("\n\t\tCount: " + (pmodel.nodeList.Count + pmodel.geomCount));
                    sb.Append("\n\t}");

                    sb.Append("\n\tObjectType: \"Geometry\" {");
                    sb.Append("\n\t\tCount: " + pmodel.geomCount);
                    sb.Append("\n\t}");

                    sb.Append("\n\tObjectType: \"Material\" {");
                    sb.Append("\n\t\tCount: " + pmodel.matList.Count);
                    sb.Append("\n\t}");

                    sb.Append("\n\tObjectType: \"Texture\" {");
                    sb.Append("\n\t\tCount: " + pmodel.texList.Count);
                    sb.Append("\n\t}");

                    sb.Append("\n\tObjectType: \"Video\" {");
                    sb.Append("\n\t\tCount: " + pmodel.texList.Count);
                    sb.Append("\n\t}");
                    sb.Append("\n}\n");

                    sb.Append("\nObjects:  {");
                    //sw.Write(sb.ToString());

                    StringBuilder cb = new StringBuilder(); //connections builder
                    cb.Append("\n}\n");//Objects end
                    cb.Append("\nConnections:  {");

                    for (int i = 0; i < pmodel.nodeList.Count; i++)
                    {
                        //sb.Length = 0;
                        var pnode = pmodel.nodeList[i];
                        var EulerRotation = ToEulerAngles(pnode.rotation);
                        int nodeID = 10000 + i; //unique number used for connections

                        cb.Append("\n\tC: \"OO\"," + nodeID + ","); //connect model to parent
                        if (pnode.parentID >= 0 && pmodel.nodeList[pnode.parentID] != null)
                        {
                            int parentID = 10000 + pnode.parentID;
                            cb.Append(parentID.ToString());
                        }
                        else { cb.Append("0"); }

                        sb.Append("\n\tModel: " + nodeID + ", \"Model::" + pnode.name + "\", \"Null\" {");
                        sb.Append("\n\t\tVersion: 232");
                        sb.Append("\n\t\tProperties70:  {");
                        sb.Append("\n\t\t\tP: \"InheritType\", \"enum\", \"\", \"\",1");
                        sb.Append("\n\t\t\tP: \"ScalingMax\", \"Vector3D\", \"Vector\", \"\",0,0,0");
                        sb.Append("\n\t\t\tP: \"DefaultAttributeIndex\", \"int\", \"Integer\", \"\",0");
                        sb.Append("\n\t\t\tP: \"Lcl Translation\", \"Lcl Translation\", \"\", \"A\"," + pnode.position[0] + "," + pnode.position[1] + "," + pnode.position[2]);
                        sb.Append("\n\t\t\tP: \"Lcl Rotation\", \"Lcl Rotation\", \"\", \"A\"," + EulerRotation[0] + "," + EulerRotation[1] + "," + EulerRotation[2]);
                        sb.Append("\n\t\t\tP: \"Lcl Scaling\", \"Lcl Scaling\", \"\", \"A\"," + pnode.scale[0] + "," + pnode.scale[1] + "," + pnode.scale[2]);
                        //sb.Append("\n\t\t\tP: \"UDP3DSMAX\", \"KString\", \"\", \"U\", \"MapChannel:1 = UVChannel_1&cr;&lf;MapChannel:2 = UVChannel_2&cr;&lf;\"");
                        sb.Append("\n\t\t\tP: \"MaxHandle\", \"int\", \"Integer\", \"UH\"," + (i + 2));
                        sb.Append("\n\t\t}");
                        sb.Append("\n\t\tShading: T");
                        sb.Append("\n\t\tCulling: \"CullingOff\"\n\t}");

                        //sw.Write(sb.ToString());
                    }
                    //sw.Write(sb.ToString());

                    for (int i = 0; i < pmodel.objectList.Count; i++)
                    {
                        var pobject = pmodel.objectList[i];
                        var parentNode = pmodel.nodeList[pobject.nodeID];

                        for (int j = 0; j < pobject.meshList.Count; j++)
                        {
                            StringBuilder vb = new StringBuilder();

                            //sb.Length = 0;
                            var pmesh = pobject.meshList[j];
                            string m_Name = parentNode.name + "_" + pmesh.materialName + "_LOD" + pmesh.LODnum.ToString();
                            int meshID = 20000 + i * 100 + j; //unique number used for connections
                            int geomID = 30000 + i * 100 + j; //unique number used for connections

                            cb.Append("\n\tC: \"OO\"," + meshID + "," + (10000 + pobject.nodeID)); //connect model to parent
                            cb.Append("\n\tC: \"OO\"," + geomID + "," + meshID); //connect geometry to model
                            cb.Append("\n\tC: \"OO\"," + (40000 + pmesh.materialID) + "," + meshID); //connect material to model

                            //write geometry first
                            sb.Append("\n\tGeometry: " + geomID + ", \"Geometry::\", \"Mesh\" {");
                            sb.Append("\n\t\tProperties70:  {");
                            var randomColor = RandomColorGenerator(m_Name);
                            sb.Append("\n\t\t\tP: \"Color\", \"ColorRGB\", \"Color\", \"\"," + ((float)randomColor[0] / 255) + "," + ((float)randomColor[1] / 255) + "," + ((float)randomColor[2] / 255));
                            sb.Append("\n\t\t}");
                            sb.Append("\n\t\tVertices: *" + pmesh.vertices.Length.ToString() + " {\n\t\t\ta: ");
                            for (int v = 0; v < pmesh.vertices.Length; v++)
                            {
                                //sb.Append(Convert.ToString(pmesh.vertices[v]).Replace(',', '.'));
                                vb.Append(pmesh.vertices[v]);
                                vb.Append(',');
                            }
                            vb.Length -= 1; //remove last ,
                            sb.Append(SplitLine(vb.ToString()));
                            sb.Append("\n\t\t}");
                            vb.Length = 0;

                            sb.Append("\n\t\tPolygonVertexIndex: *" + pmesh.indices.Length.ToString() + " {\n\t\t\ta: ");
                            for (int f = 0; f < (pmesh.indices.Length / 3); f++)
                            {
                                vb.Append(pmesh.indices[f*3]);
                                vb.Append(',');
                                vb.Append(pmesh.indices[f*3+1]);
                                vb.Append(',');
                                vb.Append(-pmesh.indices[f*3+2] - 1);
                                vb.Append(',');
                            }
                            vb.Length -= 1; //remove last ,
                            sb.Append(SplitLine(vb.ToString()));
                            sb.Append("\n\t\t}");
                            sb.Append("\n\t\tGeometryVersion: 124");
                            vb.Length = 0;

                            if (pmesh.normals != null)
                            {
                                sb.Append("\n\t\tLayerElementNormal: 0 {");
                                sb.Append("\n\t\t\tVersion: 101");
                                sb.Append("\n\t\t\tName: \"\"");
                                sb.Append("\n\t\t\tMappingInformationType: \"ByVertice\"");
                                sb.Append("\n\t\t\tReferenceInformationType: \"Direct\"");
                                sb.Append("\n\t\t\tNormals: *" + pmesh.normals.Length.ToString() + " {\n\t\t\t\ta: ");
                                for (int v = 0; v < pmesh.normals.Length; v++)
                                {
                                    //sb.Append(Convert.ToString(pmesh.normals[v]).Replace(',', '.'));
                                    vb.Append(pmesh.normals[v]);
                                    vb.Append(',');
                                }
                                vb.Length -= 1; //remove last ,
                                sb.Append(SplitLine(vb.ToString()));
                                sb.Append("\n\t\t\t}\n\t\t}");
                                vb.Length = 0;
                            }

                            if (pmesh.colors != null)
                            {
                                sb.Append("\n\t\tLayerElementColor: 0 {");
                                sb.Append("\n\t\t\tVersion: 101");
                                sb.Append("\n\t\t\tName: \"\"");
                                sb.Append("\n\t\t\tMappingInformationType: \"ByVertice\"");
                                sb.Append("\n\t\t\tReferenceInformationType: \"Direct\"");
                                sb.Append("\n\t\t\tColors: *" + pmesh.colors.Length.ToString() + " {\n\t\t\t\ta: ");
                                for (int v = 0; v < pmesh.colors.Length; v++)
                                {
                                    //sb.Append(Convert.ToString(pmesh.colors[v]).Replace(',', '.'));
                                    vb.Append(pmesh.colors[v]);
                                    vb.Append(',');
                                }
                                vb.Length -= 1; //remove last ,
                                sb.Append(SplitLine(vb.ToString()));
                                sb.Append("\n\t\t\t}\n\t\t}");
                                vb.Length = 0;
                            }

                            if (pmesh.texture0 != null)
                            {
                                sb.Append("\n\t\tLayerElementUV: 0 {");
                                sb.Append("\n\t\t\tVersion: 101");
                                sb.Append("\n\t\t\tName: \"UVChannel_1\"");
                                sb.Append("\n\t\t\tMappingInformationType: \"ByVertice\"");
                                sb.Append("\n\t\t\tReferenceInformationType: \"Direct\"");
                                sb.Append("\n\t\t\tUV: *" + pmesh.texture0.Length.ToString() + " {\n\t\t\t\ta: ");
                                for (int v = 0; v < pmesh.texture0.Length; v++)
                                {
                                    //sb.Append(Convert.ToString(pmesh.texture0[v]).Replace(',', '.'));
                                    vb.Append(pmesh.texture0[v]);
                                    vb.Append(',');
                                }
                                vb.Length -= 1; //remove last ,
                                sb.Append(SplitLine(vb.ToString()));
                                sb.Append("\n\t\t\t}\n\t\t}");
                                vb.Length = 0;
                            }

                            if (pmesh.texture1 != null)
                            {
                                sb.Append("\n\t\tLayerElementUV: 1 {");
                                sb.Append("\n\t\t\tVersion: 101");
                                sb.Append("\n\t\t\tName: \"UVChannel_2\"");
                                sb.Append("\n\t\t\tMappingInformationType: \"ByVertice\"");
                                sb.Append("\n\t\t\tReferenceInformationType: \"Direct\"");
                                sb.Append("\n\t\t\tUV: *" + pmesh.texture1.Length.ToString() + " {\n\t\t\t\ta: ");
                                for (int v = 0; v < pmesh.texture1.Length; v++)
                                {
                                    //sb.Append(Convert.ToString(pmesh.texture1[v]).Replace(',', '.'));
                                    vb.Append(pmesh.texture1[v]);
                                    vb.Append(',');
                                }
                                vb.Length -= 1; //remove last ,
                                sb.Append(SplitLine(vb.ToString()));
                                sb.Append("\n\t\t\t}\n\t\t}");
                                vb.Length = 0;
                            }

                            sb.Append("\n\t\tLayerElementMaterial: 0 {");
                            sb.Append("\n\t\t\tVersion: 101");
                            sb.Append("\n\t\t\tName: \"\"");
                            sb.Append("\n\t\t\tMappingInformationType: \"AllSame\"");
                            sb.Append("\n\t\t\tReferenceInformationType: \"IndexToDirect\"");
                            sb.Append("\n\t\t\tMaterials: *1 {");
                            sb.Append("\n\t\t\t\t0");
                            sb.Append("\n\t\t\t}");
                            sb.Append("\n\t\t}");

                            sb.Append("\n\t\tLayer: 0 {");
                            sb.Append("\n\t\t\tVersion: 100");
                            if (pmesh.normals != null)
                            {
                                sb.Append("\n\t\t\tLayerElement:  {");
                                sb.Append("\n\t\t\t\tType: \"LayerElementNormal\"");
                                sb.Append("\n\t\t\t\tTypedIndex: 0");
                                sb.Append("\n\t\t\t}");
                            }
                            sb.Append("\n\t\t\tLayerElement:  {");
                            sb.Append("\n\t\t\t\tType: \"LayerElementMaterial\"");
                            sb.Append("\n\t\t\t\tTypedIndex: 0");
                            sb.Append("\n\t\t\t}");
                            /*sb.Append("\n\t\t\tLayerElement:  {");
                            sb.Append("\n\t\t\t\tType: \"LayerElementTexture\"");
                            sb.Append("\n\t\t\t\tTypedIndex: 0");
                            sb.Append("\n\t\t\t}");
                            sb.Append("\n\t\t\tLayerElement:  {");
                            sb.Append("\n\t\t\t\tType: \"LayerElementBumpTextures\"");
                            sb.Append("\n\t\t\t\tTypedIndex: 0");
                            sb.Append("\n\t\t\t}");*/
                            if (pmesh.colors != null)
                            {
                                sb.Append("\n\t\t\tLayerElement:  {");
                                sb.Append("\n\t\t\t\tType: \"LayerElementColor\"");
                                sb.Append("\n\t\t\t\tTypedIndex: 0");
                                sb.Append("\n\t\t\t}");
                            }
                            if (pmesh.texture0 != null)
                            {
                                sb.Append("\n\t\t\tLayerElement:  {");
                                sb.Append("\n\t\t\t\tType: \"LayerElementUV\"");
                                sb.Append("\n\t\t\t\tTypedIndex: 0");
                                sb.Append("\n\t\t\t}");
                            }
                            sb.Append("\n\t\t}");
                            sb.Append("\n\t\tLayer: 1 {");
                            sb.Append("\n\t\t\tVersion: 100");
                            if (pmesh.texture1 != null)
                            {
                                sb.Append("\n\t\t\tLayerElement:  {");
                                sb.Append("\n\t\t\t\tType: \"LayerElementUV\"");
                                sb.Append("\n\t\t\t\tTypedIndex: 1");
                                sb.Append("\n\t\t\t}");
                            }
                            sb.Append("\n\t\t}");
                            //sb.Append("\n\t\tNodeAttributeName: \"Geometry::" + m_Name + "_geometry\"");
                            sb.Append("\n\t}"); //Geometry end


                            //write Model
                            sb.Append("\n\tModel: " + meshID + ", \"Model::" + m_Name + "\", \"Mesh\" {");
                            sb.Append("\n\t\tVersion: 232");
                            sb.Append("\n\t\tProperties70:  {");
                            sb.Append("\n\t\t\tP: \"InheritType\", \"enum\", \"\", \"\",1");
                            sb.Append("\n\t\t\tP: \"ScalingMax\", \"Vector3D\", \"Vector\", \"\",0,0,0");
                            sb.Append("\n\t\t\tP: \"DefaultAttributeIndex\", \"int\", \"Integer\", \"\",0");
                            sb.Append("\n\t\t\tP: \"Lcl Translation\", \"Lcl Translation\", \"\", \"A\"," + pmesh.position[0] + "," + pmesh.position[1] + "," + pmesh.position[2]);
                            //sb.Append("\n\t\t\tP: \"Lcl Rotation\", \"Lcl Rotation\", \"\", \"A\"," + EulerRotation[0] + "," + EulerRotation[1] + "," + EulerRotation[2]);
                            sb.Append("\n\t\t\tP: \"Lcl Scaling\", \"Lcl Scaling\", \"\", \"A\"," + pmesh.scale[0] + "," + pmesh.scale[1] + "," + pmesh.scale[2]);
                            //sb.Append("\n\t\t\tP: \"UDP3DSMAX\", \"KString\", \"\", \"U\", \"MapChannel:1 = UVChannel_1&cr;&lf;MapChannel:2 = UVChannel_2&cr;&lf;\"");
                            sb.Append("\n\t\t\tP: \"MaxHandle\", \"int\", \"Integer\", \"UH\"," + (j + 2 + pmodel.nodeList.Count));
                            sb.Append("\n\t\t}");
                            sb.Append("\n\t\tShading: T");
                            sb.Append("\n\t\tCulling: \"CullingOff\"");
                            sb.Append("\n\t}"); //Model end

                            //sw.Write(sb.ToString());
                        }
                    }

                    //sb.Length = 0;
                    for (int i = 0; i < pmodel.matList.Count; i++)
                    {
                        var pmat = pmodel.matList[i];

                        sb.Append("\n\tMaterial: " + (40000 + i) + ", \"Material::" + pmat.name + "\", \"\" {");
                        sb.Append("\n\t\tVersion: 102");
                        sb.Append("\n\t\tShadingModel: \"phong\"");
                        sb.Append("\n\t\tMultiLayer: 0");
                        sb.Append("\n\t\tProperties70:  {");
                        sb.Append("\n\t\t}");
                        sb.Append("\n\t}");

                        if (pmat.diffuseID >= 0)
                        {
                            cb.Append("\n\tC: \"OP\"," + (50000 + pmat.diffuseID) + "," + (40000 + i) + ", \"DiffuseColor\""); //connect texture to material

                            cb.Append("\n\tC: \"OO\"," + (60000 + pmat.diffuseID) + "," + (50000 + pmat.diffuseID)); //connect video to texture
                        }

                        if (pmat.normalID >= 0)
                        {
                            cb.Append("\n\tC: \"OP\"," + (50000 + pmat.normalID) + "," + (40000 + i) + ", \"NormalMap\""); //connect texture to material

                            cb.Append("\n\tC: \"OO\"," + (60000 + pmat.normalID) + "," + (50000 + pmat.normalID)); //connect video to texture
                        }
                    }

                    for (int i = 0; i < pmodel.texList.Count; i++)
                    {
                        var texture = pmodel.texList[i];
                        var relativeFname = MakeRelative(texture.filename, PIGfile);

                        sb.Append("\n\tTexture: " + (50000 + i) + ", \"Texture::" + texture.name + "\", \"\" {");
                        sb.Append("\n\t\tType: \"TextureVideoClip\"");
                        sb.Append("\n\t\tVersion: 202");
                        sb.Append("\n\t\tTextureName: \"Texture::" + texture.name + "\"");
                        sb.Append("\n\t\tProperties70:  {");
                        sb.Append("\n\t\t\tP: \"UVSet\", \"KString\", \"\", \"\", \"UVChannel_1\"");
                        sb.Append("\n\t\t\tP: \"UseMaterial\", \"bool\", \"\", \"\",1");
                        sb.Append("\n\t\t}");
                        sb.Append("\n\t\tMedia: \"Video::" + texture.name + "\"");
                        sb.Append("\n\t\tFileName: \"" + texture.filename + "\"");
                        sb.Append("\n\t\tRelativeFilename: \"" + relativeFname + "\"");
                        sb.Append("\n\t}");

                        sb.Append("\n\tVideo: " + (60000 + i) + ", \"Video::" + texture.name + "\", \"Clip\" {");
                        sb.Append("\n\t\tType: \"Clip\"");
                        sb.Append("\n\t\tProperties70:  {");
                        sb.Append("\n\t\t\tP: \"Path\", \"KString\", \"XRefUrl\", \"\", \"" + texture.filename + "\"");
                        sb.Append("\n\t\t}");
                        sb.Append("\n\t\tFileName: \"" + texture.filename + "\"");
                        sb.Append("\n\t\tRelativeFilename: \"" + relativeFname + "\"");
                        sb.Append("\n\t}");
                    }


                    cb.Append("\n}"); //Connections end
                    sb.Append(cb.ToString());
                    sw.Write(sb.ToString());
                    //sw.Write(cb.ToString());

                    sw.Close();
                    sw.Dispose();
                }

                converted = 1;
            }

            return converted;
        }

        public static float[] ToEulerAngles(float[] q) //every god damn time
        {
            // Store the Euler angles in radians
            float[] pitchYawRoll = new float[3] { 0, 0, 0 };

            double sqx = q[0] * q[0];
            double sqy = q[1] * q[1];
            double sqz = q[2] * q[2];
            double sqw = q[3] * q[3];

            // If quaternion is normalised the unit is one, otherwise it is the correction factor
            double unit = sqx + sqy + sqz + sqw;
            double test = -(q[0] * q[2] + q[1] * q[3]);

            if (test > 0.4999f * unit) // Singularity at north pole
            {
                pitchYawRoll[0] = -2f * (float)Math.Atan2(q[0], q[3]);  // Yaw
                pitchYawRoll[1] = (float)Math.PI * 0.5f;                         // Pitch
                pitchYawRoll[2] = 0f;                                // Roll
            }
            else if (test < -0.4999f * unit) // Singularity at south pole
            {
                pitchYawRoll[0] = -2f * (float)Math.Atan2(q[0], q[3]); // Yaw
                pitchYawRoll[1] = -(float)Math.PI * 0.5f;                        // Pitch
                pitchYawRoll[2] = 0f;                                // Roll
            }
            else
            {
                pitchYawRoll[0] = (float)Math.Atan2(2f * (q[1] * q[2] - q[0] * q[3]), -sqx - sqy + sqz + sqw);     // Yaw 
                pitchYawRoll[1] = (float)Math.Asin(2f * test / unit);                             // Pitch 
                pitchYawRoll[2] = (float)Math.Atan2(2f * (q[0] * q[1] - q[2] * q[3]), sqx - sqy - sqz + sqw);      // Roll 
            }

            pitchYawRoll[0] *= 180 / (float)Math.PI;
            pitchYawRoll[1] *= 180 / (float)Math.PI;
            pitchYawRoll[2] *= 180 / (float)Math.PI;

            return pitchYawRoll;
        }

        public static byte[] RandomColorGenerator(string name)
        {
            int nameHash = name.GetHashCode();
            Random r = new Random(nameHash);
            //Random r = new Random(DateTime.Now.Millisecond);

            byte red = (byte)r.Next(0, 255);
            byte green = (byte)r.Next(0, 255);
            byte blue = (byte)r.Next(0, 255);

            return new byte[3] { red, green, blue };
        }

        public static string MakeRelative(string filePath, string referencePath)
        {
            if (filePath != "" && referencePath != "")
            {
                var fileUri = new Uri(filePath);
                var referenceUri = new Uri(referencePath);
                return referenceUri.MakeRelativeUri(fileUri).ToString().Replace('/', Path.DirectorySeparatorChar);
            }
            else
            {
                return "";
            }
        }

        public static string SplitLine(string inputLine) //for FBX 2011
        {
            string outputLines = inputLine;
            int vbSplit = 0;
            for (int v = 0; v < inputLine.Length / 2048; v++)
            {
                vbSplit += 2048;
                if (vbSplit < outputLines.Length)
                {
                    vbSplit = outputLines.IndexOf(",", vbSplit) + 1;
                    if (vbSplit > 0) { outputLines = outputLines.Insert(vbSplit, "\n"); }
                }
            }
            return outputLines;
        }
    }
}
