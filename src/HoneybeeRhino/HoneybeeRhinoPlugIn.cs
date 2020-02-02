using Rhino;
using Rhino.Collections;
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
        public HoneybeeRhinoPlugIn()
        {
            Instance = this;
            Rhino.RhinoDoc.SelectObjects += RhinoDoc_SelectObjects;
            Rhino.RhinoDoc.EndOpenDocument += RhinoDoc_EndOpenDocument; // deal with CopyToClipboard/Paste action
            Rhino.RhinoDoc.BeforeTransformObjects += RhinoDoc_BeforeTransformObjects; //deal with Alt + Gumball drag duplicate action
            Rhino.RhinoDoc.BeginOpenDocument += RhinoDoc_BeginOpenDocument;
       
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
        }

        //this is only being used for temporary container when geometries are copy pasted in.
        private List<RhinoObject> _rhinoObjectsMergedIn = new List<RhinoObject>();

        private void RhinoDoc_SelectObjects(object sender, RhinoObjectSelectionEventArgs e)
        {
            var selectedObjs = e.RhinoObjects.Select(_=>_);

            if (this._mergedCounts>0)
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


            var selectedRooms = selectedObjs.Where(_ => _.IsRoom());
            var selectedApertures = selectedObjs.Where(_ => _.IsAperture());


            //TODO: to be changed to any of honeybee object
            if (!selectedRooms.Any())
            {
                //reset the flag.
                this._isObjectCopied = false;
                return;
            }
                


            //TODO: work on this later
            //var selectedShds = selectedObjs.Where(_ => _.IsShade());
            if (this._isObjectCopied)
            {
                //check all group entities for new copied objects.
                //TODO: figure out all new copied windows' ownership
                foreach (var newroom in selectedRooms)
                {
                    var ent = Entities.GroupEntity.RenewGroupEntity(newroom);
                    ent.ApertureIDs.AddRange(selectedApertures.Select(_ => _.Id));
                }
                //reset the flag.
                this._isObjectCopied = false;
            }
            else
            {

            }

            //Only make the room obj as the entry point for selecting the entire group entity.
            
            foreach (var room in selectedRooms)
            {
                var entity = Entities.GroupEntity.GetFromRhinoObject(room);
                if (entity != null)
                {
                    entity.SelectEntireEntity();
                    RhinoApp.WriteLine($"Room: {entity.RoomID.ToString()}; Window: {entity.ApertureIDs.Count}");

                }
                else
                {
                    //something went seriously wrong, all rooms have group entity.
                }
            }


        }

        ///<summary>Gets the only instance of the HoneybeeRhinoPlugIn plug-in.</summary>
        public static HoneybeeRhinoPlugIn Instance
        {
            get; private set;
        }

        // You can override methods here to change the plug-in behavior on
        // loading and shut down, add options pages to the Rhino _Option command
        // and maintain plug-in wide options in a document.

        protected override void ObjectPropertiesPages(List<ObjectPropertiesPage> pages)
        {
            var page = new UI.PropertyPage();
            pages.Add(page);
        }

        protected override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options)
        {
            base.ReadDocument(doc, archive, options);
        }

        protected override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options)
        {
            base.WriteDocument(doc, archive, options);
        }
    }
}