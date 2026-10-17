package com.ankamagames.dofus.logic.game.roleplay.types
{
   import avmplus.getQualifiedClassName;
   import com.ankamagames.atouin.Atouin;
   import com.ankamagames.berilia.components.Texture;
   import com.ankamagames.dofus.datacenter.spells.SpellState;
   import com.ankamagames.dofus.logic.game.fight.frames.FightEntitiesFrame;
   import com.ankamagames.dofus.logic.game.fight.managers.BuffManager;
   import com.ankamagames.dofus.logic.game.fight.types.StateBuff;
   import com.ankamagames.dofus.types.entities.AnimatedCharacter;
   import com.ankamagames.jerakine.interfaces.IRectangle;
   import com.ankamagames.jerakine.logger.Log;
   import com.ankamagames.jerakine.logger.Logger;
   import com.ankamagames.jerakine.types.Uri;
   import flash.display.Sprite;
   import flash.events.Event;
   import flash.filters.GlowFilter;
   import flash.geom.Point;
   import flash.geom.Rectangle;
   import flash.text.TextField;
   import flash.text.TextFieldAutoSize;
   import flash.text.TextFormat;
   import flash.utils.Dictionary;

   public class EntityIcon extends Sprite
   {

      private static const _log:Logger = Log.getLogger(getQualifiedClassName(EntityIcon));

      // NEKFEU : nombre d'images entre deux mises a jour des tours restants
      private static const LABEL_REFRESH_FRAMES:int = 10;

      private var _entity:AnimatedCharacter;

      private var _icons:Dictionary;

      private var _nbIcons:int;

      private var _offsets:Dictionary;

      // NEKFEU : tours restants affiches sur les icones d'etat (nom d'icone -> TextField)
      private var _labels:Dictionary;

      private var _labelFrame:int;

      public var needUpdate:Boolean;

      public var rendering:Boolean;

      public function EntityIcon(pEntity:AnimatedCharacter)
      {
         super();
         this._entity = pEntity;
         this._icons = new Dictionary(true);
         this._offsets = new Dictionary(true);
         this._labels = new Dictionary(true);
         mouseEnabled = mouseChildren = false;
         addEventListener(Event.ADDED_TO_STAGE,this.onAddedToStage);
         addEventListener(Event.REMOVED_FROM_STAGE,this.onRemovedFromStage);
      }

      public function addIcon(pIconUri:String, pIconName:String, offset:Point = null) : void
      {
         this._icons[pIconName] = new Texture();
         this._offsets[pIconName] = offset;
         var icon:Texture = this._icons[pIconName];
         icon.uri = new Uri(pIconUri);
         icon.dispatchMessages = true;
         icon.addEventListener(Event.COMPLETE,this.iconRendered);
         icon.finalize();
         ++this._nbIcons;
      }

      public function removeIcon(pIconName:String) : void
      {
         var icon:Texture = this._icons[pIconName];
         if(icon)
         {
            if(icon.parent == this)
            {
               removeChild(icon);
            }
            delete this._icons[pIconName];
            delete this._labels[pIconName];
            --this._nbIcons;
            if(numChildren == this._nbIcons)
            {
               for each(icon in this._icons)
               {
                  removeChild(icon);
               }
               for each(icon in this._icons)
               {
                  icon.x = width == 0 ? icon.width / 2 : width + 5 + icon.width / 2;
                  if(this._offsets[icon.name])
                  {
                     icon.x += this._offsets[icon.name].x;
                     icon.y += this._offsets[icon.name].y;
                  }
                  addChild(icon);
               }
               this.needUpdate = true;
            }
         }
      }

      public function hasIcon(pIconName:String) : Boolean
      {
         return this._icons[pIconName];
      }

      public function get length() : int
      {
         return this._nbIcons;
      }

      public function remove() : void
      {
         if(parent)
         {
            parent.removeChild(this);
         }
      }

      public function place(pRect:IRectangle) : void
      {
         var globalPos:Point = new Point(pRect.x + pRect.width / 2 - width * Atouin.getInstance().currentZoom / 2,pRect.y - 10 * Atouin.getInstance().currentZoom);
         if(!this._entity)
         {
            _log.warn("Trying to place an icon above an unknown entity, aborting");
            return;
         }
         if(!this._entity.parent)
         {
            _log.warn("Trying to place an icon above entity " + this._entity.name + " with no parent, aborting");
            return;
         }
         var localPos:Point = this._entity.parent.globalToLocal(globalPos);
         x = localPos.x;
         y = localPos.y;
      }

      private function iconRendered(pEvent:Event) : void
      {
         var offset:Point = null;
         var iconName:String = null;
         var icon:Texture = pEvent.currentTarget as Texture;
         icon.removeEventListener(Event.COMPLETE,this.iconRendered);
         icon.x = width == 0 ? icon.width / 2 : width + 5 + icon.width / 2;
         for(iconName in this._icons)
         {
            if(this._icons[iconName] == icon)
            {
               offset = this._offsets[iconName];
               break;
            }
         }
         if(offset)
         {
            icon.x += offset.x;
            icon.y += offset.y;
         }
         addChild(icon);
         this.needUpdate = true;
         this._labelFrame = 0;
      }

      // ===== NEKFEU : tours restants des etats =====

      private function onAddedToStage(pEvent:Event) : void
      {
         this._labelFrame = 0;
         addEventListener(Event.ENTER_FRAME,this.onEnterFrame);
      }

      private function onRemovedFromStage(pEvent:Event) : void
      {
         removeEventListener(Event.ENTER_FRAME,this.onEnterFrame);
      }

      private function onEnterFrame(pEvent:Event) : void
      {
         if(--this._labelFrame > 0)
         {
            return;
         }
         this._labelFrame = LABEL_REFRESH_FRAMES;
         try
         {
            this.updateStateLabels();
         }
         catch(e:Error)
         {
            _log.warn("Impossible de mettre a jour les tours restants : " + e.message);
         }
      }

      private function updateStateLabels() : void
      {
         var iconName:* = undefined;
         var icon:Texture = null;
         var buffs:Array = null;
         var buff:* = undefined;
         var state:SpellState = null;
         var turns:int = 0;
         var infinite:Boolean = false;
         var text:String = null;
         if(!this._entity || !FightEntitiesFrame.getCurrentInstance())
         {
            return;
         }
         buffs = BuffManager.getInstance().getAllBuff(this._entity.id);
         for(iconName in this._icons)
         {
            icon = this._icons[iconName];
            if(!icon || icon.parent != this)
            {
               continue;
            }
            turns = 0;
            infinite = false;
            if(buffs)
            {
               for each(buff in buffs)
               {
                  if(buff is StateBuff && buff.active)
                  {
                     state = SpellState.getSpellStateById(StateBuff(buff).stateId);
                     if(Boolean(state) && state.icon == iconName)
                     {
                        if(buff.duration < 0 || buff.duration >= 63)
                        {
                           infinite = true;
                        }
                        else if(buff.duration > turns)
                        {
                           turns = buff.duration;
                        }
                     }
                  }
               }
            }
            text = !infinite && turns > 0 ? String(turns) : "";
            this.setLabel(iconName,icon,text);
         }
      }

      private function setLabel(iconName:String, icon:Texture, text:String) : void
      {
         var tf:TextField = this._labels[iconName];
         var bounds:Rectangle = null;
         if(text == "")
         {
            if(tf)
            {
               if(tf.parent)
               {
                  tf.parent.removeChild(tf);
               }
               delete this._labels[iconName];
            }
            return;
         }
         if(!tf)
         {
            tf = new TextField();
            tf.selectable = false;
            tf.mouseEnabled = false;
            tf.autoSize = TextFieldAutoSize.LEFT;
            tf.defaultTextFormat = new TextFormat("Verdana",11,16777215,true);
            tf.filters = [new GlowFilter(0,1,3,3,6)];
            this._labels[iconName] = tf;
         }
         if(tf.text == text && tf.parent == icon)
         {
            return;
         }
         if(tf.parent)
         {
            tf.parent.removeChild(tf);
         }
         tf.text = text;
         bounds = icon.getBounds(icon);
         tf.x = bounds.right - tf.width + 3;
         tf.y = bounds.bottom - tf.height + 4;
         icon.addChild(tf);
      }
   }
}
