using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WorldCities
{
    [Authorize(Roles = "RegisteredUser")]
    public class HubTest: Hub
    {

    }
}
