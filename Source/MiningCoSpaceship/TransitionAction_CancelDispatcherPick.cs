using Verse;
using Verse.AI.Group;

namespace Spaceship;

public class TransitionAction_CancelDispatcherPick : TransitionAction
{
    public override void DoAction(Transition trans)
    {
        var lord = trans.target.lord;
        var thingList = (lord.LordJob as LordJob_MiningCoBase)?.targetDestination.GetThingList(lord.Map);
        if (thingList == null)
        {
            return;
        }

        foreach (var thing in thingList)
        {
            if (thing is not Building_Spaceship buildingSpaceship)
            {
                continue;
            }

            buildingSpaceship.RequestTakeOff();
            break;
        }
    }
}