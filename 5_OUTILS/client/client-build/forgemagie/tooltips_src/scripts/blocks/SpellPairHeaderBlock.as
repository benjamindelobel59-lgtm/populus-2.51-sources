package blocks
{
   import com.ankamagames.dofus.datacenter.spells.SpellPair;
   
   public class SpellPairHeaderBlock
   {
      
      private var _content:String = "";
      
      private var _block:Object;
      
      private var _spellPair:SpellPair;
      
      public function SpellPairHeaderBlock(spellPair:SpellPair)
      {
         super();
         this._spellPair = spellPair;
         this._block = Api.tooltip.createTooltipBlock(this.onAllChunkLoaded,this.getContent);
         this._block.initChunk([Api.tooltip.createChunkData("header","htmlChunks/text/content.txt")]);
      }
      
      public function onAllChunkLoaded() : void
      {
         var content:String = this._spellPair.name;
         if(Api.system.getPlayerManager().hasRights)
         {
            content += " (" + this._spellPair.id + ")";
         }
         this._content += this._block.getChunk("header").processContent({
            "classCss":"spellpair",
            "content":content
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

