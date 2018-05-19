//////////////////////////////////////////////////////
// MegaSplat - 256 texture splat mapping
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

namespace JBooth.MegaSplat.NodeEditorFramework
{
   [System.Serializable]
   [Node (false, "MegaSplat/TextureCluster")]
   public class NodeTextureCluster : Node 
   {
      public const string ID = "textureCluster";
      public override string GetID { get { return ID; } }

      public override Node Create (Vector2 pos) 
      {
         NodeTextureCluster node = CreateInstance <NodeTextureCluster> ();

         node.name = "Texture Cluster";
         node.rect = new Rect (pos.x, pos.y, 200, 195);

         NodeInput.Create(node, "Noise", "Float");
         NodeOutput.Create(node, "Texture", "Int");
         return node;
      }

      public int clusterIdx = 0;
      public Vector2 clusterNoiseScales = new Vector2(1.0f, 0.05f);
      public string propName;
      public string indexName;
      public string index2Name;
      public System.Collections.Generic.List<int> indexes;

      protected internal override void NodeGUI () 
      {
         this.rect = new Rect(rect.x, rect.y, 200, 195);
         Inputs [0].DisplayLayout (new GUIContent ("Noise", "Optional noise optimization so multiple clusters can use the same noise function"));

         if (NodeMegaSplatOutput.sConfig == null || NodeMegaSplatOutput.sConfig.clusterLibrary.Count < 1)
         {
            RTEditorGUI.TextField(new GUIContent("Error"), "Need to select the texture list in the Master node",GUI.skin.label);
            return;
         }

         var clusters = NodeMegaSplatOutput.sConfig.clusterLibrary;
         clusterIdx = Mathf.Clamp(clusterIdx, 0, clusters.Count-1);
         if (clusters[clusterIdx].previewTex == null)
         {
            clusters[clusterIdx].previewTex = new Texture2D(96, 96);
            clusters[clusterIdx].previewTex.LoadImage(clusters[clusterIdx].previewData);
         }
         RTEditorGUI.DrawTexture(clusters[clusterIdx].previewTex, 96);

         clusterIdx = RTEditorGUI.IntSlider(clusterIdx, 0, clusters.Count-1);
         RTEditorGUI.Label(clusters[clusterIdx].name);
         clusterNoiseScales.y = RTEditorGUI.FloatField("Noise Scale", clusterNoiseScales.y);
      }

      public string clusterPropName;
      public override void WriteVariables()
      {
         var data = EvalData.data;
         var name = data.GetNextName();
         Outputs[0].varName = name;

         indexName = data.WritePropertyEntry("Vector", "(0,0,0,0)", "float4");
         index2Name = data.WritePropertyEntry("Vector", "(0,0,0,0)", "float4");
         propName = data.WritePropertyEntry("Vector", "(0,1,1,0)", "float4"); // cluster index, noise scale, cluster size, user noise scale

         indexes = null;
         var config = NodeMegaSplatOutput.sConfig;
         if (config == null || config.clusterLibrary == null || config.clusterLibrary.Count <= clusterIdx)
         {
            data.Indent();
            data.sb.Append("int ");
            data.sb.Append(name);
            data.sb.Append(" = (int)");
            data.sb.Append(indexName);
            data.sb.AppendLine(".x;");
            return;
         }
         var clusters = config.clusterLibrary;
         indexes = clusters[clusterIdx].indexes;

         string noiseName = data.GetNextName();
         if (Inputs[0].connection != null)
         {
            noiseName = Inputs[0].connection.varName;
         }
         else
         {
            data.Indent();
            data.sb.Append("float ");
            data.sb.Append(noiseName);
            clusterNoiseScales.x = clusters[clusterIdx].noise.frequency;
            data.sb.AppendLine(" = FBM3D(worldPos * " + propName + ".y * " + propName + ".w" + ");");
         }

         indexes = clusters[clusterIdx].indexes;
         int count = indexes.Count;
         if (count > 8)
            count = 8;



         data.Indent();
         data.sb.Append("int ");
         data.sb.Append(name);
         data.sb.Append(" = SelectorN(");
         data.sb.Append(noiseName);
         data.sb.Append(", ");
         data.sb.Append(indexName);
         data.sb.Append(", ");
         data.sb.Append(index2Name);
         data.sb.Append(", ");
         data.sb.Append(propName);
         data.sb.AppendLine(".z);");

      } 

      public override void SetProperties(Material mat)
      {
         if (indexes == null)
            return;
         
         int count = indexes.Count;
         if (count > 8)
            count = 8;

         if (clusterNoiseScales.x < 0.001f)
            clusterNoiseScales.x = 0.05f;
         if (clusterNoiseScales.y < 0.001f)
            clusterNoiseScales.y = 0.05f;
         mat.SetVector(propName, new Vector4(clusterIdx, clusterNoiseScales.x, count, clusterNoiseScales.y));
         Vector4 i0 = Vector4.zero;
         Vector4 i1 = Vector4.zero;
         if (count > 0)
            i0.x = indexes[0];
         if (count > 1)
            i0.y = indexes[1];
         if (count > 2)
            i0.z = indexes[2];
         if (count > 3)
            i0.w = indexes[3];
         if (count > 4)
            i1.x = indexes[4];
         if (count > 5)
            i1.y = indexes[5];
         if (count > 6)
            i1.z = indexes[6];
         if (count > 7)
            i1.w = indexes[7];

         mat.SetVector(indexName, i0);
         mat.SetVector(index2Name, i1);

      }
   }
}
