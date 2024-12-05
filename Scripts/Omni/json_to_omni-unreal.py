from omni.kit.scripting import BehaviorScript
from pxr import Gf, UsdGeom, Usd
import json
import os
import omni.usd


class FileReaderScript(BehaviorScript):
    def on_init(self):
        """Initialize the script."""
        global prim1
        stage = omni.usd.get_context().get_stage()
        # Viper prim
        prim_path = "/World/Unreal_Cadre"
        prim1 = stage.GetPrimAtPath(prim_path)

        if not prim1.IsValid():
            print(f"Error: Prim not found or invalid at path {prim_path}.")
            return

        # Ensure the prim is an Xform
        prim1 = UsdGeom.Xform(prim1)
        if not prim1:
            print(f"Error: Prim at {prim_path} is not a valid Xform.")
            return

        self.file_path = r"C:\Users\Jared\Desktop\kafkaOmniConsumer_2.json"
        print(f"Script initialized. Watching file: {self.file_path}")

    def on_update(self, current_time: float, delta_time: float):
        """Read and extract specific values from the JSON file on every update."""
        try:
            if os.path.exists(self.file_path):
                with open(self.file_path, 'r') as file:
                    data = json.load(file)

                    # Extract additionalProperty values
                    additional_properties = data.get("additionalProperty", [])

                    # Create a dictionary for the values of interest
                    values_of_interest = {}
                    for prop in additional_properties:
                        name = prop.get("name", "")
                        value = prop.get("value", None)
                        if name in ["xCoordinate", "yCoordinate", "zCoordinate", "rx", "ry", "rz", "w"]:
                            values_of_interest[name] = value

                    # Print the extracted values
                    print(f"Extracted values: {values_of_interest}")

                    # Ensure required values exist
                    x = values_of_interest.get("xCoordinate", 0)
                    y = values_of_interest.get("yCoordinate", 0)
                    z = values_of_interest.get("zCoordinate", 0)
                    rx = values_of_interest.get("rx", 0)
                    ry = values_of_interest.get("ry", 0)
                    rz = values_of_interest.get("rz", 0)
                    w = values_of_interest.get("w", 1)

                    # Desired world position and rotation
                    desired_position = Gf.Vec3d(x, y, z)
                    desired_rotation = Gf.Quatf(w, rx, ry, rz)

                    # Compute and apply local transform
                    parent_prim = prim1.GetPrim().GetParent()
                    parent_world_transform = UsdGeom.Xformable(parent_prim).ComputeLocalToWorldTransform(Usd.TimeCode.Default())

                    # Handle cases where the parent doesn't have a valid transform
                    if not parent_world_transform:
                        print(f"Warning: Parent prim at {parent_prim.GetPath()} has no transform. Using identity.")
                        parent_world_transform = Gf.Matrix4d(1.0)  # Identity matrix

                    parent_world_inverse = parent_world_transform.GetInverse()
                    local_position = parent_world_inverse.TransformAffine(desired_position)

                    # Apply transformations to the prim
                    xformable = UsdGeom.Xformable(prim1)

                    # Clear existing ops and apply the new transform
                    xformable.ClearXformOpOrder()
                    translate_op = xformable.AddTranslateOp()
                    translate_op.Set(local_position)

                    orient_op = xformable.AddOrientOp()
                    orient_op.Set(desired_rotation)

                    print(f"Prim updated: Local Position {local_position}, Rotation {desired_rotation}")

            else:
                print(f"File not found: {self.file_path}")
        except Exception as e:
            print(f"Error reading or updating prim: {e}")

    def on_shutdown(self):
        """Clean up resources if necessary."""
        print("Shutting down script.")
