package blocks
{
   import d2hooks.*;
   import flash.xml.XMLNode;
   import flash.xml.XMLNodeType;
   
   public class ConditionTooltipBlock
   {
      
      private var _conditions:Object;
      
      private var _content:String;
      
      private var _titleText:String;
      
      private var _onTarget:Boolean;
      
      private var _block:Object;
      
      private var _customli:String;
      
      private var _effectCssClass:String;
      
      public function ConditionTooltipBlock(conditions:Object, titleText:String = null, onTarget:Boolean = false, chunkType:String = "chunks", customli:String = "customlirightmargin", effectCssClass:String = "requirement")
      {
         super();
         this._conditions = conditions;
         this._onTarget = onTarget;
         this._customli = customli;
         this._effectCssClass = effectCssClass;
         if(titleText)
         {
            this._titleText = titleText;
         }
         else
         {
            this._titleText = Api.ui.getText("ui.common.conditions");
         }
         this._block = Api.tooltip.createTooltipBlock(this.onAllChunkLoaded,this.getContent);
         this._block.initChunk([Api.tooltip.createChunkData("subTitle",chunkType + "/base/subTitle.txt"),Api.tooltip.createChunkData("effect",chunkType + "/effect/effect.txt")]);
      }
      
      public function onAllChunkLoaded() : void
      {
         var criterion:Object = null;
         var index:uint = 0;
         var criteriaText:String = null;
         var criteriaRespected:Boolean = false;
         var ORindex:int = 0;
         var i:int = 0;
         var firstParenthesisIndex:int = 0;
         var logicOperatorIndex:int = 0;
         var newLineOperator:String = null;
         var inlineCriteria:Object = null;
         if(this._conditions == null || this._conditions.criteria == null || this._conditions.criteria.length == 0)
         {
            return;
         }
         this._content = this._block.getChunk("subTitle").processContent({"text":this._titleText});
         var tempContent:String = "";
         for each(criterion in this._conditions.criteria)
         {
            index = 0;
            criteriaText = "";
            criteriaRespected = Boolean(criterion.isRespected);
            ORindex = int(criterion.text.indexOf("|"));
            firstParenthesisIndex = 0;
            logicOperatorIndex = 0;
            for(i = 0; i < criterion.inlineCriteria.length; i++)
            {
               inlineCriteria = criterion.inlineCriteria[i];
               if(inlineCriteria.text != "")
               {
                  criteriaText = "";
                  if(index > 0)
                  {
                     if(ORindex > 0)
                     {
                        criteriaText = Api.ui.getText("ui.common.or") + " ";
                     }
                  }
                  if(this._onTarget)
                  {
                     criteriaText = "(" + Api.ui.getText("ui.item.target") + ") ";
                  }
                  if(!criteriaRespected && !inlineCriteria.isRespected && this._effectCssClass == "requirement")
                  {
                     this._effectCssClass = "malus";
                     criteriaText += inlineCriteria.text;
                  }
                  else
                  {
                     criteriaText += inlineCriteria.text;
                  }
                  if(criterion.inlineCriteria.length > 1 && i == firstParenthesisIndex && Boolean(this._conditions.operators))
                  {
                     criteriaText = "(" + criteriaText;
                  }
                  if(newLineOperator == "|")
                  {
                     criteriaText = Api.ui.getText("ui.common.or") + " " + criteriaText;
                     newLineOperator = "null";
                  }
                  else if(newLineOperator == "&")
                  {
                     criteriaText = Api.ui.getText("ui.common.and") + " " + criteriaText;
                     newLineOperator = "null";
                  }
                  if(criterion.inlineCriteria.length > 1)
                  {
                     if(i != firstParenthesisIndex && i == criterion.inlineCriteria.length - 1 && Boolean(this._conditions.operators))
                     {
                        criteriaText += ")";
                        if(Boolean(this._conditions.operators) && this._conditions.operators.length > logicOperatorIndex)
                        {
                           newLineOperator = this._conditions.operators[logicOperatorIndex];
                        }
                        logicOperatorIndex++;
                     }
                  }
                  else if(this._conditions.criteria.length > 1 && Boolean(this._conditions.operators))
                  {
                     if(this._conditions.operators[logicOperatorIndex] == "|")
                     {
                        newLineOperator = this._conditions.operators[logicOperatorIndex];
                        logicOperatorIndex++;
                     }
                  }
                  index++;
               }
               else if(i == 0)
               {
                  firstParenthesisIndex++;
               }
               if(criteriaText)
               {
                  criteriaText = XML(new XMLNode(XMLNodeType.TEXT_NODE,criteriaText)).toXMLString();
                  tempContent += this._block.getChunk("effect").processContent({
                     "text":criteriaText,
                     "cssClass":this._effectCssClass,
                     "li":"",
                     "customli":this._customli
                  },{"valueCssClass":"value"});
               }
            }
         }
         if(tempContent == "")
         {
            this._content = "";
         }
         else
         {
            this._content += tempContent;
         }
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

