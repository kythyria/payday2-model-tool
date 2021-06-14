using PD2ModelParser.Misc;
using PD2ModelParser.Sections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PD2ModelParser.Importers {
    class AnimationImporter {
        public static void Import(FullModelData fmd, string path) {
            AnimationFile animationFile = new AnimationFile();
            animationFile.Read(path);

            foreach(AnimationFileObject animationObject in animationFile.Objects) {
                Object3D object3D = fmd.GetObject3DByHash(new HashName(animationObject.Name));

                Log.Default.Info("Trying to add animation to " + animationObject.Name);
                if (object3D != null) {
                    Log.Default.Info("Found " + animationObject.Name);

                    object3D.Animations.Clear(); // Kill the old anims.

                    if (animationObject.RotationKeyframes.Count > 0) {
                        QuatLinearRotationController quatLinearRotationController = AddRotations(object3D, animationObject.RotationKeyframes);
                        fmd.AddSection(quatLinearRotationController);
                    }

                    if (animationObject.PositionKeyframes.Count > 0) {
                        LinearVector3Controller linearVector3Controller = AddPositions(object3D, animationObject.PositionKeyframes);
                        fmd.AddSection(linearVector3Controller);
                    }
                } else {
                    Log.Default.Info("Not Found " + animationObject.Name);
                }
            }
        }

        public static QuatLinearRotationController AddRotations(Object3D targetObject, IList<Keyframe<Quaternion>> keyframes) {
            var quatLinearRotationController = new QuatLinearRotationController();
            quatLinearRotationController.Keyframes = new List<Keyframe<Quaternion>>(keyframes);
            quatLinearRotationController.KeyframeLength = quatLinearRotationController.Keyframes.Max(kf => kf.Timestamp);

            if (targetObject.Animations.Count == 0) {
                targetObject.Animations.Add(quatLinearRotationController);
                targetObject.Animations.Add(null);
            } else if (targetObject.Animations.Count == 1 && (targetObject.Animations[0].GetType() == typeof(LinearVector3Controller))) {
                targetObject.Animations.Insert(0, quatLinearRotationController);
            } else {
                throw new Exception($"Failed to insert animation in {targetObject.Name}: unrecognised controller list shape");
            }

            return quatLinearRotationController;
        }

        public static LinearVector3Controller AddPositions(Object3D targetObject, IList<Keyframe<Vector3>> keyframes) {
            LinearVector3Controller linearVector3Controller = new LinearVector3Controller();
            linearVector3Controller.Keyframes = new List<Keyframe<Vector3>>(keyframes);
            linearVector3Controller.KeyframeLength = linearVector3Controller.Keyframes.Max(kf => kf.Timestamp);

            if (targetObject.Animations.Count == 0) {
                targetObject.Animations.Add(linearVector3Controller);
            } else if (targetObject.Animations.Count == 2
                  && targetObject.Animations[0].GetType() == typeof(QuatLinearRotationController)
                  && targetObject.Animations[1] == null) {
                targetObject.Animations[1] = linearVector3Controller;
            } else {
                throw new Exception($"Failed to insert animation in {targetObject.Name}: unrecognised controller list shape");
            }

            return linearVector3Controller;
        }
    }
}
