using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Attacking : FSMState<Enemy>
{
    float biteTimer;

    public Attacking (FSM<Enemy> FSMOwner) : base (FSMOwner)
    { }

    public override void Begin(Enemy obj)
    {
        throw new NotImplementedException();
    }

    public override void Update(Enemy obj)
    {
        throw new NotImplementedException();
    }

    public override void End(Enemy obj)
    {
        throw new NotImplementedException();
    }
}

