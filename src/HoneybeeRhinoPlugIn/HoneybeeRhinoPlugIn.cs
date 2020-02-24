﻿using HoneybeeRhino.Entities;
using HoneybeeRhino;
using Rhino;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.UI;
using System.Collections.Generic;
using System.Linq;

namespace HoneybeeRhino
{
    ///<summary>
    /// <para>Every RhinoCommon .rhp assembly must have one and only one PlugIn-derived
    /// class. DO NOT create instances of this class yourself. It is the
    /// responsibility of Rhino to create an instance of this class.</para>
    /// <para>To complete plug-in information, please also see all PlugInDescription
    /// attributes in AssemblyInfo.cs (you might need to click "Project" ->
    /// "Show All Files" to see it in the "Solution Explorer" window).</para>
    ///</summary>
    public class HoneybeeRhinoPlugIn : Rhino.PlugIns.PlugIn
    {
        private RoomEntityMouseCallback m_mc;

        //TODO: add GroupEntityTable into ModelEntity
        public GroupEntityTable GroupEntityTable { get; private set; } = new GroupEntityTable();
        //TODO: need to take care of saving to doc
        public ModelEntityTable ModelEntityTable { get; private set; } = new ModelEntityTable();
        public override Rhino.PlugIns.PlugInLoadTime LoadTime => Rhino.PlugIns.PlugInLoadTime.AtStartup;
        public string ObjectSelectMode { get; set; } = "GroupEntity";

        public HoneybeeRhinoPlugIn()
        {
            Instance = this;
            Rhino.RhinoDoc.SelectObjects += RhinoDoc_SelectObjects;
            Rhino.RhinoDoc.EndOpenDocument += RhinoDoc_EndOpenDocument; // deal with CopyToClipboard/Paste action
            Rhino.RhinoDoc.BeforeTransformObjects += RhinoDoc_BeforeTransformObjects; //deal with Alt + Gumball drag duplicate action
            Rhino.RhinoDoc.BeginOpenDocument += RhinoDoc_BeginOpenDocument;
            Rhino.RhinoDoc.CloseDocument += RhinoDoc_OnCloseDocument;


            if (m_mc == null)
            {
                m_mc = new RoomEntityMouseCallback();
                m_mc.Enabled = true;
            }

        }



        private bool _isObjectCopied = false;
        private int _mergedCounts = 0;
        private void RhinoDoc_BeforeTransformObjects(object sender, RhinoTransformObjectsEventArgs e)
        {
            //dealing with Alt + Gumball drag duplicate action
            var isCopied = e.ObjectsWillBeCopied;

            if (isCopied)
            {
                this._isObjectCopied = isCopied;
                Rhino.RhinoApp.WriteLine($"{e.ObjectCount} will be copied: {e.ObjectsWillBeCopied}");
            }

        }
        private void RhinoDoc_BeginOpenDocument(object sender, DocumentOpenEventArgs e)
        {
            this._mergedCounts = e.Document.Objects.Count;
        }
        private void RhinoDoc_EndOpenDocument(object sender, DocumentOpenEventArgs e)
        {
            if (e.FileName.EndsWith("tmp") && e.Merge)
            {
                //this is a copy/paste action
                this._isObjectCopied = true;
                this._mergedCounts = e.Document.Objects.Count - this._mergedCounts;
                Rhino.RhinoApp.WriteLine($"Total {this._mergedCounts} objects merged");
            }
            else
            {
                this._isObjectCopied = true;
                this._mergedCounts = 0;
            }
        }

        private void RhinoDoc_OnCloseDocument(object sender, DocumentEventArgs e)
        {
            // When the document is closed, clear our 
            // document user data containers.
            GroupEntityTable.Clear();
        }


        //this is only being used for temporary container when geometries are copy pasted in.
        private List<RhinoObject> _rhinoObjectsMergedIn = new List<RhinoObject>();

        private void RhinoDoc_SelectObjects(object sender, RhinoObjectSelectionEventArgs e)
        {
            var selectedObjs = e.RhinoObjects.Select(_ => _);

            if (this._mergedCounts > 0)
            {
                //Get all pasted in objects one by one.
                while (this._mergedCounts > _rhinoObjectsMergedIn.Count + 1)
                {
                    this._rhinoObjectsMergedIn.AddRange(selectedObjs);
                    return;
                }
                //Get the last one
                this._rhinoObjectsMergedIn.AddRange(selectedObjs);
                //All pasted objects
                selectedObjs = this._rhinoObjectsMergedIn.GetRange(0, this._mergedCounts);

                //reset the counts.
                this._mergedCounts = 0;
                this._rhinoObjectsMergedIn.Clear();
            }

            //var selectedGroupEntities = selectedObjs.Select(_ => GroupEntity.TryGet(_));
            var selectedRooms = selectedObjs.Where(_ => _.Geometry.IsRoom()).Select(_ => _ as BrepObject);
            var selectedApertures = selectedObjs.Where(_ => _.Geometry.IsAperture()).Select(_=>_ as BrepObject);
            //TODO: work on this later
            //var selectedShds = selectedObjs.Where(_ => _.IsShade());


            if (this._isObjectCopied)
            {
                //check all group entities for new copied objects.
                foreach (var newAperture in selectedApertures)
                {
                    //TODO: refactor this later 
                    newAperture.TryGetApertureEntity().UpdateHostFrom(newAperture);
                }
              

                //TODO: figure out all new copied windows' ownership
                foreach (var newroom in selectedRooms)
                {
                    var roomEnt = newroom.TryGetRoomEntity();
                    roomEnt.UpdateHostID(newroom, Instance.GroupEntityTable);
                    
                    var grpEnt = GroupEntity.TryGetFromID(newroom.Id, Instance.GroupEntityTable);
                    grpEnt.AddApertures(selectedApertures);
                }
                //reset the flag.
                this._isObjectCopied = false;
            }
            else
            {

            }

            //currently there is one object is double clicked,
            //it is under editing, and no need to select the entire groupEntity.
            if (!this.m_mc.EditingObj.Equals(System.Guid.Empty))
            {
                return;
            }


            //Only make the room obj as the entry point for selecting the entire group entity.
            foreach (var room in selectedRooms)
            {
                var entity = room.Geometry.TryGetGroupEntity(Instance.GroupEntityTable);
                if (entity.IsValid)
                {
                    entity.SelectEntireEntity();
                    entity.SelectShades();
                }
                else
                {
                    //ignore
                }
                RhinoApp.WriteLine($"Room: {entity.Guid.ToString()}; Window: {entity.ApertureCount}");
            }
            foreach (var apt in selectedApertures)
            {
                var entity = apt.Geometry.TryGetGroupEntity(Instance.GroupEntityTable);
                if (entity.IsValid)
                {
                    entity.SelectRoom();

                }
                else
                {
                    //ignore
                }
            }



        }

        ///<summary>Gets the only instance of the HoneybeeRhinoPlugIn plug-in.</summary>
        public static HoneybeeRhinoPlugIn Instance
        {
            get; private set;
        }


        protected override void ObjectPropertiesPages(List<ObjectPropertiesPage> pages)
        {
            var page = new global::HoneybeeRhino.UI.PropertyPage();
            pages.Add(page);
        }


        protected override bool ShouldCallWriteDocument(FileWriteOptions options)
        {
            return !options.WriteGeometryOnly && !options.WriteSelectedObjectsOnly;
        }


        protected override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options)
        {
            archive.Read3dmChunkVersion(out var major, out var minor);
            if (major == 1 && minor == 0)
            {
                var t = new GroupEntityTable();
                t.ReadDocument(archive);

                if (!options.ImportMode && !options.ImportReferenceMode)
                {
                    if (t.Count > 0)
                        GroupEntityTable = t;

                }
            }
        }

        protected override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options)
        {
            archive.Write3dmChunkVersion(1, 0);
            this.GroupEntityTable.WriteDocument(archive);

        }
    }
}