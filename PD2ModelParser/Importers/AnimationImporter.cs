using PD2ModelParser.Misc;
using PD2ModelParser.Sections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser.Importers {
	class AnimationImporter {
        public static void Import(FullModelData fmd, string path) {
            AnimationFile animationFile = new AnimationFile();
            animationFile.Read(path);

            foreach(AnimationFileObject animationObject in animationFile.Objects) {
                Object3D object3D = fmd.GetObject3DByHash(new HashName(animationObject.Name));

				Log.Default.Info("Trying to add animation to " + object3D.Name);
				if (object3D != null) {
					Log.Default.Info("Added animation to " + object3D.Name);

					// For some reason, they've gotta be this way round, Rotation first, Position after. :shrug:

					if (animationObject.RotationKeyframes.Count > 0) {
                        QuatLinearRotationController quatLinearRotationController = new QuatLinearRotationController();
                        quatLinearRotationController.Keyframes = new List<Keyframe<Quaternion>>(animationObject.RotationKeyframes);

                        fmd.AddSection(quatLinearRotationController);

                        object3D.Animations.Add(quatLinearRotationController);
                    }

                    if (animationObject.PositionKeyframes.Count > 0) {
                        LinearVector3Controller linearVector3Controller = new LinearVector3Controller();
                        linearVector3Controller.Keyframes = new List<Keyframe<Vector3>>(animationObject.PositionKeyframes);

                        fmd.AddSection(linearVector3Controller);

                        object3D.Animations.Add(linearVector3Controller);
                    }
                }
            }
        }
    }
}
