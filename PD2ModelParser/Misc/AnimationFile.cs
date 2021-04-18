using Collada141;
using PD2ModelParser.Misc.ZLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using PD2ModelParser.Sections;

namespace PD2ModelParser.Misc {
	class AnimationFileObject {
		public string Name;
		public IList<Keyframe<Vector3>> PositionKeyframes = new List<Keyframe<Vector3>>();
		public IList<Keyframe<Quaternion>> RotationKeyframes = new List<Keyframe<Quaternion>>();

        public AnimationFileObject(string name) {
            this.Name = name;
		}
    }

    class AnimationFile {
        public List<AnimationFileObject> Objects = new List<AnimationFileObject>();

        public static void ZLibDecompress(Stream stream, Stream tragetStream) {
            using (var zs = new ZLIBStream(stream, CompressionMode.Decompress, true)) {
                int bytesLeidos = 0;
                byte[] buffer = new byte[1024];

                while ((bytesLeidos = zs.Read(buffer, 0, buffer.Length)) > 0)
                    tragetStream.Write(buffer, 0, bytesLeidos);
            }
        }

        public static void ZLibCompress(Stream stream, Stream targetStream, CompressionLevel? level = null) {
            using (var zs = new ZLIBStream(targetStream, level ?? CompressionLevel.Optimal, true)) {
                int bytesLeidos = 0;
                byte[] buffer = new byte[1024];

                stream.Seek(0, SeekOrigin.Begin);

                while ((bytesLeidos = stream.Read(buffer, 0, buffer.Length)) > 0)
                    zs.Write(buffer, 0, bytesLeidos);
            }

            using (var bw = new BinaryWriter(targetStream))
                bw.Write((uint)stream.Length);
        }

        public void Read(string filePath) {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            Read(fs);
        }

        public void Read(FileStream fs) {
            MemoryStream ms = new MemoryStream();
            ZLibDecompress(fs, ms);
            BinaryReader br = new BinaryReader(ms);
            ReadRaw(br);
        }

        public void ReadRaw(BinaryReader br) {
            long savePos = 0;

            // Header Shit
            br.BaseStream.Seek(20, SeekOrigin.Begin);

            uint objectNameCount = br.ReadUInt32();
            uint objectNameOffset = br.ReadUInt32();

            br.ReadUInt32();
            br.ReadUInt32();
            br.ReadUInt32();
            br.ReadUInt32();

            uint objectPositionsCount = br.ReadUInt32();
            uint objectPositionsOffset = br.ReadUInt32();

            uint objectRotationsCount = br.ReadUInt32();
            uint objectRotationsOffset = br.ReadUInt32();

            // Objects
            br.BaseStream.Seek(objectNameOffset, SeekOrigin.Begin);
            for (int i = 0; i < objectNameCount; i++) {
                uint nameOffset = br.ReadUInt32();
                savePos = br.BaseStream.Position;

                br.BaseStream.Seek(nameOffset, SeekOrigin.Begin);
                List<char> s = new List<char>();
                char c = ' ';
                while (c != '\0') {
                    c = br.ReadChar();
                    if (c != '\0')
                        s.Add(c);
                }
                Objects.Add(new AnimationFileObject(new string(s.ToArray())));

                br.BaseStream.Seek(savePos, SeekOrigin.Begin);
            }

            // Positions
            br.BaseStream.Seek(objectPositionsOffset, SeekOrigin.Begin);
            for (int i = 0; i < objectPositionsCount; i++) {
                AnimationFileObject currentObject = Objects[i];

                uint positionType = br.ReadUInt32();
                uint positionOffset = br.ReadUInt32();
                savePos = br.BaseStream.Position;

                br.BaseStream.Seek(positionOffset, SeekOrigin.Begin);
                br.ReadUInt32();
                uint positionCount = br.ReadUInt32();
                br.BaseStream.Seek(br.ReadUInt32(), SeekOrigin.Begin);

                for (int n = 0; n < positionCount; n++) {
                    float timestamp = br.ReadSingle();
                    switch (positionType) {
                        case (499549920):
                            Vector3 vector3 = new Vector3(
                                ((br.ReadUInt16() / 65535) * 200) - 100,
                                ((br.ReadUInt16() / 65535) * 200) - 100,
                                ((br.ReadUInt16() / 65535) * 200) - 100
                            );
                            br.ReadUInt16(); // Blank Unknown Skip
                            currentObject.PositionKeyframes.Add(new Keyframe<Vector3>(timestamp, vector3));
                            break;
                        case (295096242):
                            Vector3 vector = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                            if (br.ReadUInt32() == 0) {
                                currentObject.PositionKeyframes.Add(new Keyframe<Vector3>(
                                    timestamp,
                                    vector
                                ));
                            };
                            break;
                    }
                }

                br.BaseStream.Seek(savePos, SeekOrigin.Begin);
            }

            // Rotations
            br.BaseStream.Seek(objectRotationsOffset, SeekOrigin.Begin);
            for (int i = 0; i < objectRotationsCount; i++) {
                AnimationFileObject currentObject = Objects[i];

                uint rotationType = br.ReadUInt32();
                uint rotationOffset = br.ReadUInt32();
                savePos = br.BaseStream.Position;

                br.BaseStream.Seek(rotationOffset, SeekOrigin.Begin);
                br.ReadUInt32();
                uint rotationCount = br.ReadUInt32();
                br.BaseStream.Seek(br.ReadUInt32(), SeekOrigin.Begin);

                for (int n = 0; n < rotationCount; n++) {
                    float timestamp = br.ReadSingle();
                    switch (rotationType) {
                        case (2650510006):
                            currentObject.RotationKeyframes.Add(new Keyframe<Quaternion>(
                                timestamp,
                                new Quaternion(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle())
                            ));
                            break;
                        case (3910822330):
                            Quaternion quaternion = new Quaternion(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                            if (br.ReadUInt32() == 0) {
                                currentObject.RotationKeyframes.Add(new Keyframe<Quaternion>(
                                    timestamp,
                                    quaternion
                                ));
                            }
                            break;
                    }
                }

                br.BaseStream.Seek(savePos, SeekOrigin.Begin);
            }
        }

        public void Write(string filePath) {
            FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            Write(fs);
            fs.Close();
        }

        public void Write(Stream stream) {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            WriteRaw(bw);

            ZLibCompress(ms, stream);
            bw.Close();
            ms.Close();
        }

        public void WriteRaw(BinaryWriter bw) {
            bw.Write(0x85CC8308);
            bw.Write((ulong)0);

            int objectNameSize = Objects.Count * 4;
            int objectPositionsSize = Objects.Count * 20;
            int objectRotationsSize = Objects.Count * 20;            

            float highestTime = 0f;
            foreach (AnimationFileObject animationObject in Objects) {
                objectNameSize += animationObject.Name.Length + 1;
                objectPositionsSize += animationObject.PositionKeyframes.Count * 20;
                objectRotationsSize += animationObject.RotationKeyframes.Count * 20;

                foreach (Keyframe<Vector3> keyframe in animationObject.PositionKeyframes) {
                    if (keyframe.Timestamp > highestTime) { highestTime = keyframe.Timestamp; }
                }
                foreach (Keyframe<Quaternion> keyframe in animationObject.RotationKeyframes) {
                    if (keyframe.Timestamp > highestTime) { highestTime = keyframe.Timestamp; }
                }
            }
            bw.Write(60 + objectNameSize + objectPositionsSize + objectRotationsSize); // Filesize
            bw.Write(highestTime);

            bw.Write(Objects.Count);
            long nameOffsetPos = bw.BaseStream.Position;
            bw.Write(60);

            // Useless for exporting boring animations.
            bw.Write(0);
            bw.Write(0);
            bw.Write(0);
            bw.Write(0);

            bw.Write(Objects.Count);
            long posOffsetPos = bw.BaseStream.Position;
            bw.Write(60);

            bw.Write(Objects.Count);
            long rotOffsetPos = bw.BaseStream.Position;
            bw.Write(60);

            // Names
            long lastOffsetPos = bw.BaseStream.Position;
            bw.BaseStream.Seek(nameOffsetPos, SeekOrigin.Begin);
            bw.Write((uint) lastOffsetPos);
            bw.BaseStream.Seek(lastOffsetPos, SeekOrigin.Begin);

            for (int i = 0; i < Objects.Count; i++) {
                bw.Write(0);
            }

            for (int i = 0; i < Objects.Count; i++) {
                long lastDataPos = bw.BaseStream.Position;
                string name = Objects[i].Name;
                for (int x = 0; x < name.Length; x++) {
                    bw.Write(name[x]);
                }
                bw.Write((byte)0);

                long returnPos = bw.BaseStream.Position;
                bw.BaseStream.Seek(lastOffsetPos + (i * 4), SeekOrigin.Begin);
                bw.Write((uint) (lastDataPos));
                bw.BaseStream.Seek(returnPos, SeekOrigin.Begin);
            }

            // Positions
            lastOffsetPos = bw.BaseStream.Position;
            bw.BaseStream.Seek(posOffsetPos, SeekOrigin.Begin);
            bw.Write((uint)lastOffsetPos);
            bw.BaseStream.Seek(lastOffsetPos, SeekOrigin.Begin);

            for (int i = 0; i < Objects.Count; i++) {
                bw.Write(295096242);
                bw.Write(0);
            }

            for (int i = 0; i < Objects.Count; i++) {
                long lastDataPos = bw.BaseStream.Position;

                IList<Keyframe<Vector3>> positions = Objects[i].PositionKeyframes;

                bw.Write(0);
                bw.Write(positions.Count);
                bw.Write((uint)(bw.BaseStream.Position + 4));

                foreach ( Keyframe<Vector3> keyframe in positions) {
                    Vector3 position = keyframe.Value;

                    bw.Write(keyframe.Timestamp);
                    bw.Write(position.X);
                    bw.Write(position.Y);
                    bw.Write(position.Z);
                    bw.Write(0);
                }

                long returnPos = bw.BaseStream.Position;
                bw.BaseStream.Seek(lastOffsetPos + (i * 8) + 4, SeekOrigin.Begin);
                bw.Write((uint)(lastDataPos));
                bw.BaseStream.Seek(returnPos, SeekOrigin.Begin);
            }

            // Rotation
            lastOffsetPos = bw.BaseStream.Position;
            bw.BaseStream.Seek(rotOffsetPos, SeekOrigin.Begin);
            bw.Write((uint)lastOffsetPos);
            bw.BaseStream.Seek(lastOffsetPos, SeekOrigin.Begin);

            for (int i = 0; i < Objects.Count; i++) {
                bw.Write(2650510006);
                bw.Write(0);
            }

            for (int i = 0; i < Objects.Count; i++) {
                long lastDataPos = bw.BaseStream.Position;

                IList<Keyframe<Quaternion>> rotations = Objects[i].RotationKeyframes;

                bw.Write(0);
                bw.Write(rotations.Count);
                bw.Write((uint)(bw.BaseStream.Position + 4));

                foreach (Keyframe<Quaternion> keyframe in rotations) {
                    Quaternion rotation = keyframe.Value;

                    bw.Write(keyframe.Timestamp);
                    bw.Write(rotation.X);
                    bw.Write(rotation.Y);
                    bw.Write(rotation.Z);
                    bw.Write(rotation.W);
                }

                long returnPos = bw.BaseStream.Position;
                bw.BaseStream.Seek(lastOffsetPos + (i * 8) + 4, SeekOrigin.Begin);
                bw.Write((uint)(lastDataPos));
                bw.BaseStream.Seek(returnPos, SeekOrigin.Begin);
            }
        }
    }
}
