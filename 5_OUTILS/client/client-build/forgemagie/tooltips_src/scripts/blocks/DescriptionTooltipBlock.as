package blocks
{
   import flash.xml.XMLNode;
   import flash.xml.XMLNodeType;
   
   public class DescriptionTooltipBlock
   {
      
      private var _description:String;
      
      private var _content:String;
      
      private var _cssClass:String;
      
      private var _block:Object;
      
      public function DescriptionTooltipBlock(description:String, cssClass:String = "description", chunkType:String = "chunks")
      {
         super();
         this._description = description;
         this._cssClass = cssClass;
         this._block = Api.tooltip.createTooltipBlock(this.onAllChunkLoaded,this.getContent);
         this._block.initChunk([Api.tooltip.createChunkData("description",chunkType + "/description/description.txt")]);
      }
      
      public function onAllChunkLoaded() : void
      {
         var descriptionText:String = XML(new XMLNode(XMLNodeType.TEXT_NODE,this._description)).toXMLString();
         this._content = this._block.getChunk("description").processContent({
            "description":descriptionText,
            "cssClass":this._cssClass
         });
      }
      
      public function getContent() : String
      {
         return this._content;
      }
      
      public function get block() : Object
      {
         return this._block;
      }
   }
}

