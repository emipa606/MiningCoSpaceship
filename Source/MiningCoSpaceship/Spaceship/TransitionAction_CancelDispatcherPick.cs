using Verse;
using Verse.AI.Group;

namespace Spaceship;

public class TransitionAction_CancelDispatcherPick : TransitionAction
{
    public override void DoAction(Transition trans)
    {
        var lord = trans.target.lord;
        var thinglist = (lord.LordJob as LordJob_MiningCoBase)?.targetDestination.GetThingList(lord.Map);
        if (thinglist == null)
        {
            return;
        }

        foreach (var thing in thinglist)
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