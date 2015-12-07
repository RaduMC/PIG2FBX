using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using Lz4;

namespace PIG2FBX
{
    public class PIGmodel
    {
        public List<PIGnode> nodeList = new List<PIGnode>();
        public List<PIGobject> objectList = new List<PIGobject>();
        public List<PIGmaterial> matList = new List<PIGmaterial>();
        public List<PIGtexture> texList = new List<PIGtexture>();

        public int usedTexCount = 0;
        public int geomCount = 0;

        public class PIGnode
        {
            public string name;
            public short parentID;
            public float[] position = new float[3] { 0, 0, 0 };
            public float[] rotation = new float[4] { 0, 0, 0, 1 };
            public float[] scale = new float[3] { 1, 1, 1 };
        }

        public class PIGobject
        {
            public int nodeID;
            public List<PIGmesh> meshList = new List<PIGmesh>();
        }

        public class PIGmesh
        {
            public byte LODnum;
            public float[] position = new float[3] { 0, 0, 0 };
            public float[] scale = new float[3] { 1, 1, 1 };
            public ushort vertexCount;
            public string materialName;
            public int materialID;
            public float[] vertices;
            public float[] normals;
            public float[] colors;
            public float[] texture0;
            public float[] texture1;
            public ushort[] indices;
        }

        public class PIGmaterial
        {
            public string name;
            public int diffuseID = -1;
            public int normalID = -1;
        }

        public class PIGtexture
        {
            public string name;
            public string filename = "";
        }

        public class MatIComparer : IComparer<PIGmaterial>
        {
            public int Compare(PIGmaterial x, PIGmaterial y)
            {
                return x.name.CompareTo(y.name);
            }
        }
        private MatIComparer mc = new MatIComparer();

        public class TexIComparer : IComparer<PIGtexture>
        {
            public int Compare(PIGtexture x, PIGtexture y)
            {
                return x.name.CompareTo(y.name);
            }
        }
        private TexIComparer tc = new TexIComparer();

        public PIGmodel(string PIGfile)
        {
            using (BinaryReader binStream = new BinaryReader(File.OpenRead(PIGfile)))
            {
                int marker = binStream.ReadInt32(); //0x64
                short nodeCount = binStream.ReadInt16();

                for (int n = 0; n < nodeCount; n++)
                {
                    PIGnode newnode = new PIGnode();

                    marker = binStream.ReadInt32();
                    newnode.name = ReadStr(binStream);
                    byte nbyte = binStream.ReadByte();
                    newnode.parentID = binStream.ReadInt16();

                    newnode.position[0] = binStream.ReadSingle();
                    newnode.position[1] = binStream.ReadSingle();
                    newnode.position[2] = binStream.ReadSingle();
                    newnode.rotation[0] = binStream.ReadSingle();
                    newnode.rotation[1] = binStream.ReadSingle();
                    newnode.rotation[2] = binStream.ReadSingle();
                    newnode.rotation[3] = binStream.ReadSingle();
                    newnode.scale[0] = binStream.ReadSingle();
                    newnode.scale[1] = binStream.ReadSingle();
                    newnode.scale[2] = binStream.ReadSingle();

                    float afloat = binStream.ReadSingle();
                    short ashort = binStream.ReadInt16();

                    nodeList.Add(newnode);
                }


                byte abyte = binStream.ReadByte();
                short objectCount = binStream.ReadInt16();

                for (int o = 0; o < objectCount; o++)
                {
                    PIGobject newobject = new PIGobject();

                    marker = binStream.ReadInt32(); //0x64
                    newobject.nodeID = binStream.ReadInt32();
                    int LODcount = binStream.ReadInt16();

                    for (int l = 0; l < LODcount; l++)
                    {
                        byte LODnum = binStream.ReadByte();
                        marker = binStream.ReadInt32(); //0x64
                        short edata = binStream.ReadInt16();
                        binStream.BaseStream.Position += 24; //bounding box
                        short meshCount = binStream.ReadInt16();
                        geomCount += meshCount;

                        for (int m = 0; m < meshCount; m++)
                        {
                            PIGmesh newmesh = new PIGmesh();
                            newmesh.LODnum = LODnum;

                            marker = binStream.ReadInt32(); //0x64
                            BitArray bitflags = new BitArray(new int[1] {binStream.ReadInt32()});
                            int FVFcode = binStream.ReadInt32();
                            binStream.BaseStream.Position += 12; //mpivot

                            if (bitflags[0])
                            {
                                newmesh.position[0] = binStream.ReadSingle();
                                newmesh.position[1] = binStream.ReadSingle();
                                newmesh.position[2] = binStream.ReadSingle();
                                newmesh.scale[0] = binStream.ReadSingle();
                                newmesh.scale[1] = binStream.ReadSingle();
                                newmesh.scale[2] = binStream.ReadSingle();
                            }

                            ushort vertexCount = binStream.ReadUInt16();
                            newmesh.vertexCount = vertexCount;
                            int indexCount = binStream.ReadInt32();
                            string materialName = ReadStr(binStream);
                            newmesh.materialName = materialName;
                            newmesh.materialID = matList.Count;

                            PIGmaterial pmat = new PIGmaterial();
                            pmat.name = materialName;

                            short texureCount = binStream.ReadInt16();
                            for (int t = 0; t < texureCount; t++)
                            {
                                string textureName = ReadStr(binStream);

                                if (textureName.Length > 0)
                                {
                                    PIGtexture ptex = new PIGtexture();
                                    ptex.name = textureName;
                                    int ptexID = texList.Count;
                                    var existingTexIndex = texList.BinarySearch(ptex, tc);
                                    if (existingTexIndex >= 0) { ptexID = existingTexIndex; }
                                    else
                                    {
                                        //get filename
                                        string[] texFiles = Directory.GetFiles(Path.GetDirectoryName(PIGfile) + "\\..\\", Path.GetFileNameWithoutExtension(textureName) + ".pvr", SearchOption.AllDirectories);
                                        if (texFiles.Length == 0)
                                        {
                                            texFiles = Directory.GetFiles(Path.GetDirectoryName(PIGfile) + "\\..\\", textureName, SearchOption.AllDirectories);
                                            if (texFiles.Length > 0)
                                            {
                                                var pvrfile = DecompressPVR(texFiles[0]);
                                                ptex.filename = pvrfile;
                                            }
                                        }
                                        else { ptex.filename = Path.GetFullPath(texFiles[0]); }

                                        texList.Add(ptex);
                                    }

                                    switch (t)
                                    {
                                        case 0: //diffuse
                                            pmat.diffuseID = ptexID;
                                            usedTexCount += 1;
                                            break;
                                        case 2: //normal
                                            pmat.normalID = ptexID;
                                            usedTexCount += 1;
                                            break;
                                    }
                                }
                            }

                            //add material only if it wasn't added before
                            var existingMatIndex = matList.BinarySearch(pmat, mc);
                            if (existingMatIndex >= 0)
                            {
                                newmesh.materialID = existingMatIndex;
                            }
                            else { matList.Add(pmat); }


                            abyte = binStream.ReadByte();
                            int bufferSize = binStream.ReadInt32();
                            byte[] geobuffer = new byte[bufferSize];

                            if (bufferSize == 0) //lz4 compression
                            {
                                int compressedSize = binStream.ReadInt32();
                                int uncompressedSize = binStream.ReadInt32();

                                byte[] lz4buffer = new byte[compressedSize];
                                binStream.Read(lz4buffer, 0, compressedSize);

                                using (var inputStream = new MemoryStream(lz4buffer))
                                {
                                    var decoder = new Lz4DecoderStream(inputStream);

                                    geobuffer = new byte[uncompressedSize]; //is this ok?
                                    for (; ; )
                                    {
                                        int nRead = decoder.Read(geobuffer, 0, geobuffer.Length);
                                        if (nRead == 0)
                                            break;
                                    }
                                }
                                /*using (BinaryWriter debugStream = new BinaryWriter(File.Open((Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + "_" + o + "_" + l + "_" + m), FileMode.Create)))
                                {
                                    debugStream.Write(geobuffer);
                                    debugStream.Close();
                                }*/
                            }
                            else { binStream.Read(geobuffer, 0, bufferSize); }

                            newmesh.indices = new ushort[indexCount];

                            using (BinaryReader geostream = new BinaryReader(new MemoryStream(geobuffer)))
                            {
                                geostream.BaseStream.Position = 0; //is this needed?

                                #region positions
                                if ((FVFcode | 1) == FVFcode) //positions
                                {
                                    newmesh.vertices = new float[vertexCount * 3];
                                    short align = geostream.ReadInt16();
                                    geostream.BaseStream.Position += align;

                                    //test if components are 16bit shorts or 32bit floats
                                    int predictedAlign = 16 - ((vertexCount * 8) % 16) - 2;
                                    geostream.BaseStream.Position += vertexCount * 8;
                                    align = geostream.ReadInt16();
                                    geostream.BaseStream.Position -= vertexCount * 8 + 2;

                                    if (align != predictedAlign)
                                    {
                                        for (int v = 0; v < vertexCount * 3; v++)
                                        {
                                            newmesh.vertices[v] = geostream.ReadSingle();
                                        }
                                    }
                                    else
                                    {
                                        for (int v = 0; v < vertexCount; v++)
                                        {
                                            newmesh.vertices[v * 3] = (float)geostream.ReadInt16() / 32767;
                                            newmesh.vertices[v * 3 + 1] = (float)geostream.ReadInt16() / 32767;
                                            newmesh.vertices[v * 3 + 2] = (float)geostream.ReadInt16() / 32767;
                                            geostream.BaseStream.Position += 2; //w component
                                        }
                                    }

                                }
                                #endregion

                                #region normals
                                if ((FVFcode | 2) == FVFcode) //normals
                                {
                                    newmesh.normals = new float[vertexCount * 3];
                                    short align = geostream.ReadInt16();
                                    geostream.BaseStream.Position += align;

                                    for (int v = 0; v < vertexCount; v++)
                                    {
                                        newmesh.normals[v * 3] = (float)geostream.ReadSByte() / 127;
                                        newmesh.normals[v * 3 + 1] = (float)geostream.ReadSByte() / 127;
                                        newmesh.normals[v * 3 + 2] = (float)geostream.ReadSByte() / 127;
                                        geostream.BaseStream.Position += 1;
                                    }
                                }
                                #endregion

                                #region misc
                                if ((FVFcode | 4) == FVFcode) //tangents
                                {
                                    short align = geostream.ReadInt16();
                                    geostream.BaseStream.Position += align;
                                    geostream.BaseStream.Position += vertexCount * 4;
                                }

                                if ((FVFcode | 8) == FVFcode) //??
                                {
                                    short align = geostream.ReadInt16();
                                    geostream.BaseStream.Position += align;
                                    geostream.BaseStream.Position += vertexCount * 4;
                                }

                                if ((FVFcode | 16) == FVFcode) //??
                                {
                                    short align = geostream.ReadInt16();
                                    geostream.BaseStream.Position += align;
                                    geostream.BaseStream.Position += vertexCount * 4;
                                }

                                if ((FVFcode | 32) == FVFcode) //??
                                {
                                    short align = geostream.ReadInt16();
                                    geostream.BaseStream.Position += align;
                                    geostream.BaseStream.Position += vertexCount * 4;
                                }

                                if ((FVFcode | 64) == FVFcode) //not colors
                                {
                                    //newmesh.colors = new float[vertexCount * 4];
                                    short align = geostream.ReadInt16();
                                    geostream.BaseStream.Position += align;
                                    geostream.BaseStream.Position += vertexCount * 4;
                                    /*for (int v = 0; v < vertexCount * 4; v++)
                                    {
                                        newmesh.colors[v] = (float)geostream.ReadByte() / 255;
                                    }*/
                                }
                                #endregion

                                #region texture coords
                                if ((FVFcode | 128) == FVFcode) //texture0
                                {
                                    newmesh.texture0 = new float[vertexCount * 2];
                                    short align = geostream.ReadInt16();
                                    geostream.BaseStream.Position += align;

                                    //test if components are 16bit shorts or 32bit floats
                                    int predictedAlign = 16 - ((vertexCount * 4) % 16) - 2;
                                    geostream.BaseStream.Position += vertexCount * 4;
                                    align = geostream.ReadInt16();
                                    geostream.BaseStream.Position -= vertexCount * 4 + 2;

                                    if (align != predictedAlign)
                                    {
                                        for (int v = 0; v < vertexCount; v++)
                                        {
                                            newmesh.texture0[v*2] = geostream.ReadSingle();
                                            newmesh.texture0[v*2 + 1] = 1f - geostream.ReadSingle();
                                        }
                                    }
                                    else
                                    {
                                        for (int v = 0; v < vertexCount; v++)
                                        {
                                            newmesh.texture0[v*2] = (float)geostream.ReadInt16() / 32767;
                                            newmesh.texture0[v*2 + 1] = 1f - (float)geostream.ReadInt16() / 32767;
                                        }
                                    }

                                }

                                if ((FVFcode | 256) == FVFcode) //texture1
                                {
                                    newmesh.texture1 = new float[vertexCount * 2];
                                    short align = geostream.ReadInt16();
                                    geostream.BaseStream.Position += align;

                                    //test if components are 16bit shorts or 32bit floats
                                    int predictedAlign = 16 - ((vertexCount * 4) % 16) - 2;
                                    geostream.BaseStream.Position += vertexCount * 4;
                                    align = geostream.ReadInt16();
                                    geostream.BaseStream.Position -= vertexCount * 4 + 2;

                                    if (align != predictedAlign)
                                    {
                                        for (int v = 0; v < vertexCount; v++)
                                        {
                                            newmesh.texture1[v*2] = geostream.ReadSingle();
                                            newmesh.texture1[v*2] = 1f - geostream.ReadSingle();
                                        }
                                    }
                                    else
                                    {
                                        for (int v = 0; v < vertexCount; v++)
                                        {
                                            newmesh.texture1[v*2] = (float)geostream.ReadInt16() / 32767;
                                            newmesh.texture1[v*2] = 1f - (float)geostream.ReadInt16() / 32767;
                                        }
                                    }

                                }
                                #endregion

                                #region misc2
                                if ((FVFcode | 512) == FVFcode) //??
                                {
                                    short align = geostream.ReadInt16();
                                    geostream.BaseStream.Position += align;

                                    int predictedAlign = 16 - ((vertexCount * 4) % 16) - 2;
                                    geostream.BaseStream.Position += vertexCount * 4;
                                    align = geostream.ReadInt16();
                                    geostream.BaseStream.Position -= 2;
                                    if (align != predictedAlign) { geostream.BaseStream.Position += vertexCount * 4; } //happened in car_ford_mustang_2015_rfx.pig
                                }

                                if ((FVFcode | 1024) == FVFcode) //??
                                {
                                    short align = geostream.ReadInt16();
                                    geostream.BaseStream.Position += align;
                                    geostream.BaseStream.Position += vertexCount * 8;
                                }
                                #endregion

                                geostream.BaseStream.Position = geostream.BaseStream.Length - (indexCount * 2); //failsafe in case vertex properties are misread
                                //note that IB alignment is skipped
                                for (int i = 0; i < indexCount; i++)
                                {
                                    newmesh.indices[i] = geostream.ReadUInt16();
                                }
                            }
                            

                            for (int e = 0; e < edata; e++)
                            {
                                short short1 = binStream.ReadInt16();
                                short short2 = binStream.ReadInt16();
                                int extraSize = binStream.ReadInt32();
                                if (extraSize == 0)
                                {
                                    int compressedSize = binStream.ReadInt32();
                                    int uncompressedSize = binStream.ReadInt32();
                                    binStream.BaseStream.Position += compressedSize;
                                }
                                else { binStream.BaseStream.Position += extraSize; }
                            }

                            newobject.meshList.Add(newmesh);
                        }
                    }

                    objectList.Add(newobject);
                }
            }
        }

        public string ReadStr(BinaryReader str)
        {
            int len = str.ReadInt16();
            byte[] stringData = new byte[len];
            str.Read(stringData, 0, len);
            var result = System.Text.Encoding.UTF8.GetString(stringData);
            return result;
        }

        public class lz4mip
        {
            public int offset;
            public int compressedSize;
            public int uncompressedSize;
        }

        public string DecompressPVR(string tgaFile)
        {
            string outFile = Path.GetDirectoryName(tgaFile) + "\\" + Path.GetFileNameWithoutExtension(tgaFile);
            bool compressed = false;

            using (BinaryReader tgaStream = new BinaryReader(File.OpenRead(tgaFile)))
            {
                int head = tgaStream.ReadInt32();

                switch (head)
                {
                    case 1481919403:
                        {
                            outFile += ".ktx";
                            break;
                        }
                    case 55727696:
                        {
                            outFile += ".pvr";
                            int imageSize = 52;
                            tgaStream.BaseStream.Position = 44;
                            int mipCount = tgaStream.ReadInt32();
                            int metaSize = tgaStream.ReadInt32();
                            if (metaSize > 0)
                            {
                                int JETtest = tgaStream.ReadInt32();
                                int LZ4test = tgaStream.ReadInt32();
                                if (JETtest == 1413827072 && LZ4test == 878332928) //lz4
                                {
                                    compressed = true;
                                    List<lz4mip> lz4mipData = new List<lz4mip>();

                                    int dataSize = tgaStream.ReadInt32();
                                    for (int i = 0; i < mipCount; i++)
                                    {//assume no surfaces or faces
                                        lz4mip amip = new lz4mip();
                                        amip.offset = tgaStream.ReadInt32();
                                        amip.compressedSize = tgaStream.ReadInt32();
                                        amip.uncompressedSize = tgaStream.ReadInt32();
                                        imageSize += amip.uncompressedSize;
                                        lz4mipData.Add(amip);
                                    }

                                    byte[] imageBuffer = new byte[imageSize];
                                    tgaStream.BaseStream.Position = 0;
                                    tgaStream.Read(imageBuffer, 0, 48); //excluding meta data size which will be 0
                                    int imageOffset = 52;

                                    foreach (var amip in lz4mipData)
                                    {
                                        tgaStream.BaseStream.Position = 52 + metaSize + amip.offset;
                                        byte[] lz4buffer = new byte[amip.compressedSize];
                                        tgaStream.Read(lz4buffer, 0, amip.compressedSize);

                                        using (var inputStream = new MemoryStream(lz4buffer))
                                        {
                                            var decoder = new Lz4DecoderStream(inputStream);

                                            byte[] mipBuffer = new byte[amip.uncompressedSize];
                                            for (; ; )
                                            {
                                                int nRead = decoder.Read(mipBuffer, 0, amip.uncompressedSize);
                                                if (nRead == 0)
                                                    break;
                                            }

                                            Buffer.BlockCopy(mipBuffer, 0, imageBuffer, imageOffset, amip.uncompressedSize);
                                            imageOffset += amip.uncompressedSize;
                                        }
                                    }

                                    using (BinaryWriter pvrStream = new BinaryWriter(File.Open(outFile, FileMode.Create)))
                                    {
                                        pvrStream.Write(imageBuffer);
                                        pvrStream.Close();
                                    }

                                    return outFile;
                                }
                            }
                            break;
                        }
                }

                
            }

            if (!compressed)
            {
                System.IO.File.Move(tgaFile, outFile);
                return outFile;
            }
            else { return tgaFile; }
        }
    }
}
