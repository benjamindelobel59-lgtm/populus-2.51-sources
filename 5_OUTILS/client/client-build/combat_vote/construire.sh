#!/bin/bash
# PADGREF 23/10 : construit les 4 DofusInvoker (L64/L32 = clients locaux, V64/V32 = clients du VPS) :
#   classes principales (vitesse ×2 : DofusFpsManager, AnimatedMovementBehavior, DofusOptions) + module Fight 427 (bouton Replay)
#   + module Config 422 (64 bits : boutons No anim / Vitesse de la barre)
#   + FightEventsHelper (journal de combat : « tous les alliés / ennemis » au lieu d'une ligne par cible)
cd "$(dirname "$0")"
J="../../../logiciels/Java17/bin/java.exe"; F="../../../logiciels/FFDec/ffdec.jar"
P=scripts/com/ankamagames
for v in L64 L32 V64 V32; do
  b=${v:1:2}; s=patch$b/$P
  "$J" -jar "$F" -air -replace base/$v.swf tmp1_$v.swf com.ankamagames.dofus.logic.common.managers.DofusFpsManager $s/dofus/logic/common/managers/DofusFpsManager.as \
     com.ankamagames.atouin.entities.behaviours.movements.AnimatedMovementBehavior $s/atouin/entities/behaviours/movements/AnimatedMovementBehavior.as \
     com.ankamagames.dofus.types.DofusOptions $s/dofus/types/DofusOptions.as \
     com.ankamagames.dofus.logic.game.fight.fightEvents.FightEventsHelper $s/dofus/logic/game/fight/fightEvents/FightEventsHelper.as || exit 1
  "$J" -jar "$F" -replace tmp1_$v.swf tmp2_$v.swf 427 Fight.replay.swf || exit 1
  if [ "$b" = "64" ]; then "$J" -jar "$F" -replace tmp2_$v.swf DofusInvoker.$v.combat-vote.swf 422 Config64.noanim.swf || exit 1; else mv tmp2_$v.swf DofusInvoker.$v.combat-vote.swf; fi
  rm -f tmp1_$v.swf tmp2_$v.swf
done
ls -la DofusInvoker.*.combat-vote.swf
