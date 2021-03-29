// Server Only Script

public class EvilLordBaseController : MonsterBase1Controller
{
    public bool isMonsterAlive()
    {
        return monster1 == null || monster1Status.IsAlive();
    }
}
