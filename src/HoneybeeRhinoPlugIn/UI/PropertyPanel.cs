using Eto.Drawing;
using Eto.Forms;
using Rhino.UI;
using System;
using System.Linq;
using HoneybeeSchema;
using HoneybeeRhino.Entities;

namespace HoneybeeRhino.UI
{
    internal class PropertyPanel: Panel
    {
        public PropertyPanel()
        {
        }


        public void updateFacePanel(FaceEntity hbObjEntity)
        {

            var face = hbObjEntity.HBObject;
            var layout = new DynamicLayout { };
            layout.Spacing = new Size(5, 5);
            layout.Padding = new Padding(10);
            layout.DefaultSpacing = new Size(2, 2);


            layout.AddSeparateRow(new Label { Text = $"ID: {face.Name}" });
            
            layout.AddSeparateRow(new Label { Text = "Name:" });
            var nameTBox = new TextBox() { };
            face.DisplayName = face.DisplayName ?? string.Empty;
            nameTBox.TextBinding.Bind(face, m => m.DisplayName );
            layout.AddSeparateRow(nameTBox);

            
            layout.AddSeparateRow(new Label { Text = "Face Type:" });
            var terrainTypeDP = new EnumDropDown<Face.FaceTypeEnum>();
            terrainTypeDP.SelectedValueBinding.Bind(Binding.Delegate(() => face.FaceType, v => face.FaceType = v));
            layout.AddSeparateRow(terrainTypeDP);


            layout.AddSeparateRow(new Label { Text = "Boundary Condition: (WIP)" });
            var bcTBox = new TextBox() { };
            bcTBox.Enabled = false;
            bcTBox.TextBinding.Bind(face, m => m.BoundaryCondition.Obj.GetType().Name);
            layout.AddSeparateRow(bcTBox);


            layout.AddSeparateRow(new Label { Text = "Properties:" });
            var faceRadPropBtn = new Button { Text = "Face Radiance Properties (WIP)" };
            faceRadPropBtn.Click += (s, e) => Dialogs.ShowMessage("Work in progress", "Honeybee");
            layout.AddSeparateRow(faceRadPropBtn);
            var faceEngPropBtn = new Button { Text = "Face Energy Properties" };
            faceEngPropBtn.Click += (s, e) => FacePropBtn_Click(hbObjEntity);
            layout.AddSeparateRow(faceEngPropBtn);

            
            var apertureLBox = new ListBox();
            apertureLBox.Height = 100;
            var apertures = hbObjEntity.ApertureObjRefs;
            var faceCount = 0;
            if (apertures != null)
            {
                var validApertures = apertures.Where(_ => _.TryGetApertureEntity().IsValid);
                faceCount = validApertures.Count();

                var faceItems = validApertures.Select(_ => _.TryGetApertureEntity().HBObject).Select(_ => new ListItem() { Text = _.DisplayName ?? _.Name, Tag = _ });
                apertureLBox.Items.AddRange(faceItems);
               
            }
            layout.AddSeparateRow(new Label { Text = $"Apertures: (total: {faceCount})" });
            layout.AddSeparateRow(apertureLBox);

            layout.AddSeparateRow(new Label { Text = $"Doors:" });
            var doorLBox = new ListBox();
            doorLBox.Height = 50;
            var doors = face.Doors;
            if (doors != null)
            {
                var faceItems = doors.Select(_ => new ListItem() { Text = _.DisplayName ?? _.Name, Tag = _ });
                doorLBox.Items.AddRange(faceItems);
            }
            layout.AddSeparateRow(doorLBox);

            layout.AddSeparateRow(new Label { Text = "IndoorShades:" });
            var inShadesListBox = new ListBox();
            inShadesListBox.Height = 50;
            var inShds = face.IndoorShades;
            if (inShds != null)
            {
                var idShds = inShds.Select(_ => new ListItem() { Text = _.DisplayName ?? _.Name, Tag = _ });
                inShadesListBox.Items.AddRange(idShds);
            }
            layout.AddSeparateRow(inShadesListBox);

            layout.AddSeparateRow(new Label { Text = "OutdoorShades:" });
            var outShadesListBox = new ListBox();
            outShadesListBox.Height = 50;
            var outShds = face.OutdoorShades;
            if (outShds != null)
            {
                var outShdItems = outShds.Select(_ => new ListItem() { Text = _.DisplayName ?? _.Name, Tag = _ });
                outShadesListBox.Items.AddRange(outShdItems);
            }
            layout.AddSeparateRow(outShadesListBox);


            layout.Add(null);
            var data_button = new Button { Text = "Honeybee Data" };
            data_button.Click += (sender, e) => Dialogs.ShowEditBox("Honeybee Data", "Honeybee Data can be shared across all platforms.", face.ToJson(), true, out string outJson);
            layout.AddSeparateRow(data_button, null);


            this.Content = layout;
            //layout.up

            void FacePropBtn_Click(Entities.FaceEntity ent)
            {
                var energyProp = ent.HBObject.Properties.Energy ?? new FaceEnergyPropertiesAbridged();
                energyProp = FaceEnergyPropertiesAbridged.FromJson(energyProp.ToJson());
                var dialog = new UI.Dialog_FaceEnergyProperty(energyProp);
                dialog.RestorePosition();
                var dialog_rc = dialog.ShowModal(RhinoEtoApp.MainWindow);
                dialog.SavePosition();
                if (dialog_rc != null)
                {
                    //replace brep in order to add an undo history
                    var undo = Rhino.RhinoDoc.ActiveDoc.BeginUndoRecord("Set Honeybee face energy properties");

                    //Dup entire room for replacement
                    var dupRoomHost = ent.RoomHostObjRef.Brep().DuplicateBrep();
                    //get face entity with subsurface component index
                    var dupEnt = dupRoomHost.Faces[ent.ComponentIndex.Index].TryGetFaceEntity();
                    //update properties
                    dupEnt.HBObject.Properties.Energy = dialog_rc;
                    Rhino.RhinoDoc.ActiveDoc.Objects.Replace(ent.RoomHostObjRef.ObjectId, dupRoomHost);

                    Rhino.RhinoDoc.ActiveDoc.EndUndoRecord(undo);

                }

            }
        }

        public void updateAperturePanel(ApertureEntity hbObjEntity)
        {

            var apt = hbObjEntity.HBObject;
            var layout = new DynamicLayout { };
            layout.Spacing = new Size(5, 5);
            layout.Padding = new Padding(10);
            layout.DefaultSpacing = new Size(2, 2);


            layout.AddSeparateRow(new Label { Text = $"ID: {apt.Name}" });

            layout.AddSeparateRow(new Label { Text = "Name:" });
            var nameTBox = new TextBox() { };
            apt.DisplayName = apt.DisplayName ?? string.Empty;
            nameTBox.TextBinding.Bind(apt, m => m.DisplayName);
            layout.AddSeparateRow(nameTBox);


            layout.AddSeparateRow(new Label { Text = "Operable:" });
            var operableCBox = new CheckBox();
            operableCBox.CheckedBinding.Bind(apt, v => v.IsOperable);
            layout.AddSeparateRow(operableCBox);


            layout.AddSeparateRow(new Label { Text = "Boundary Condition: (WIP)" });
            var bcTBox = new TextBox() { };
            bcTBox.Enabled = false;
            bcTBox.TextBinding.Bind(apt, m => m.BoundaryCondition.Obj.GetType().Name);
            layout.AddSeparateRow(bcTBox);


            layout.AddSeparateRow(new Label { Text = "Properties:" });
            var faceRadPropBtn = new Button { Text = "Face Radiance Properties (WIP)" };
            faceRadPropBtn.Click += (s, e) => Dialogs.ShowMessage("Work in progress", "Honeybee");
            layout.AddSeparateRow(faceRadPropBtn);
            var faceEngPropBtn = new Button { Text = "Face Energy Properties" };
            faceEngPropBtn.Click += (s, e) => PropBtn_Click(hbObjEntity);
            layout.AddSeparateRow(faceEngPropBtn);



            layout.AddSeparateRow(new Label { Text = "IndoorShades:" });
            var inShadesListBox = new ListBox();
            inShadesListBox.Height = 50;
            var inShds = apt.IndoorShades;
            if (inShds != null)
            {
                var idShds = inShds.Select(_ => new ListItem() { Text = _.DisplayName ?? _.Name, Tag = _ });
                inShadesListBox.Items.AddRange(idShds);
            }
            layout.AddSeparateRow(inShadesListBox);

            layout.AddSeparateRow(new Label { Text = "OutdoorShades:" });
            var outShadesListBox = new ListBox();
            outShadesListBox.Height = 50;
            var outShds = apt.OutdoorShades;
            if (outShds != null)
            {
                var outShdItems = outShds.Select(_ => new ListItem() { Text = _.DisplayName ?? _.Name, Tag = _ });
                outShadesListBox.Items.AddRange(outShdItems);
            }
            layout.AddSeparateRow(outShadesListBox);


            layout.Add(null);
            var data_button = new Button { Text = "Honeybee Data" };
            data_button.Click += (sender, e) => Dialogs.ShowEditBox("Honeybee Data", "Honeybee Data can be shared across all platforms.", apt.ToJson(), true, out string outJson);
            layout.AddSeparateRow(data_button, null);


            this.Content = layout;
            //layout.up

            void PropBtn_Click(Entities.ApertureEntity ent)
            {
                var energyProp = ent.HBObject.Properties.Energy ?? new ApertureEnergyPropertiesAbridged();
                energyProp = ApertureEnergyPropertiesAbridged.FromJson(energyProp.ToJson());
                var dialog = new UI.Dialog_ApertureEnergyProperty(energyProp);
                dialog.RestorePosition();
                var dialog_rc = dialog.ShowModal(RhinoEtoApp.MainWindow);
                dialog.SavePosition();
                if (dialog_rc != null)
                {
                    //replace brep in order to add an undo history
                    var undo = Rhino.RhinoDoc.ActiveDoc.BeginUndoRecord("Set Honeybee aperture energy properties");

                    var dup = ent.HostObjRef.Brep().DuplicateBrep();
                    dup.TryGetApertureEntity().HBObject.Properties.Energy = dialog_rc;
                    Rhino.RhinoDoc.ActiveDoc.Objects.Replace(ent.HostObjRef.ObjectId, dup);

                    Rhino.RhinoDoc.ActiveDoc.EndUndoRecord(undo);

                }

            }
        }


        public void updateRoomPanel(RoomEntity hbObjEntity)
        {

            var room = hbObjEntity.GetHBRoom();
            var layout = new DynamicLayout { };
            layout.Spacing = new Size(5, 5);
            layout.Padding = new Padding(10);
            layout.DefaultSpacing = new Size(2, 2);


            layout.AddSeparateRow(new Label { Text = $"ID: {room.Name}" });

            layout.AddSeparateRow(new Label { Text = "Name:" });
            var modelNameTextBox = new TextBox() { };
            room.DisplayName = room.DisplayName ?? string.Empty;
            modelNameTextBox.TextBinding.Bind(room, m => m.DisplayName );
            layout.AddSeparateRow(modelNameTextBox);


            layout.AddSeparateRow(new Label { Text = "Properties:" });
            var rmPropBtn = new Button { Text = "Room Energy Properties" };
            rmPropBtn.Click += (s, e) => RmPropBtn_Click(hbObjEntity);
            layout.AddSeparateRow(rmPropBtn);

            layout.AddSeparateRow(new Label { Text = $"Faces: (total: {room.Faces.Count})" });
            var facesListBox = new ListBox();
            facesListBox.Height = 120;
            var faces = room.Faces;
            if (faces != null)
            {
                var faceItems = faces.Select(_ => new ListItem() { Text = _.DisplayName ?? _.Name, Tag = _ });
                facesListBox.Items.AddRange(faceItems);
            }
            layout.AddSeparateRow(facesListBox);


            layout.AddSeparateRow(new Label { Text = "IndoorShades:" });
            var inShadesListBox = new ListBox();
            inShadesListBox.Height = 50;
            var inShds = room.IndoorShades;
            if (inShds != null)
            {
                var idShds = inShds.Select(_ => new ListItem() { Text = _.DisplayName ?? _.Name, Tag = _ });
                inShadesListBox.Items.AddRange(idShds);
            }
            layout.AddSeparateRow(inShadesListBox);

            layout.AddSeparateRow(new Label { Text = "OutdoorShades:" });
            var outShadesListBox = new ListBox();
            outShadesListBox.Height = 50;
            var outShds = room.OutdoorShades;
            if (outShds != null)
            {
                var outShdItems = outShds.Select(_ => new ListItem() { Text = _.DisplayName ?? _.Name, Tag = _ });
                outShadesListBox.Items.AddRange(outShdItems);
            }
            layout.AddSeparateRow(outShadesListBox);


            layout.AddSeparateRow(new Label { Text = "Multiplier:" });
            var multiplierNum = new NumericMaskedTextBox<int>() { };
            multiplierNum.TextBinding.Bind(Binding.Delegate(() => room.Multiplier.ToString(), v => room.Multiplier = CheckIfNum(v)));
            int CheckIfNum(string numString)
            {
                var isNum = int.TryParse(numString, out int numValue);
                return isNum ? numValue : 0;
            }
            layout.AddSeparateRow(multiplierNum);



            layout.Add(null);
            var data_button = new Button { Text = "Honeybee Data" };
            data_button.Click += (sender, e) => Dialogs.ShowEditBox("Honeybee Data", "Honeybee Data can be shared across all platforms.", room.ToJson(), true, out string outJson);
            layout.AddSeparateRow(data_button, null);


            this.Content = layout;
            //layout.up

            void RmPropBtn_Click(Entities.RoomEntity roomEnt)
            {
                var roomEnergyProperties = RoomEnergyPropertiesAbridged.FromJson(roomEnt.GetEnergyProp().ToJson());
                var dialog = new UI.Dialog_RoomEnergyProperty(roomEnergyProperties);
                dialog.RestorePosition();
                var dialog_rc = dialog.ShowModal(RhinoEtoApp.MainWindow);
                dialog.SavePosition();
                if (dialog_rc != null)
                {
                    //replace brep in order to add an undo history
                    var undo = Rhino.RhinoDoc.ActiveDoc.BeginUndoRecord("Set Honeybee room energy properties");

                    var dup = roomEnt.HostObjRef.Brep().DuplicateBrep();
                    dup.TryGetRoomEntity().GetHBRoom().Properties.Energy = dialog_rc;
                    Rhino.RhinoDoc.ActiveDoc.Objects.Replace(roomEnt.HostObjRef.ObjectId, dup);

                    Rhino.RhinoDoc.ActiveDoc.EndUndoRecord(undo);

                }

            }
        }
    

    }
}
