# External Libraries

If you are building the model tool with FBX support enabled, you'll need to place
the relevant files here - the FBX SDK, the FbxNET native component and the FbxNET
managed component.

Either build them yourself from [FbxNET](https://gitlab.com/znixian/fbxnet), or
download the binaries from GitLab CI and AppVeyor (for Windows and Linux, respectively).

If you don't want to do this, and don't need FBX support, then you can build the program
under the DebugNoFbx configuration. As it's name implies, you don't need the FBX libraries
for this.
