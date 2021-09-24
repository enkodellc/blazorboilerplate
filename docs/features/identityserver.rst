IdentityServer4
===============

`IdentityServer4 <https://identityserver4.readthedocs.io/en/latest/>`_ has detailed documentation to read first.

IdentityServer4 authentication is used with ApplicationController:

::

    [Route("api/data/[action]")]
    [Authorize(AuthenticationSchemes = AuthSchemes)]
    [BreezeQueryFilter]
    public class ApplicationController : Controller
    {
        private const string AuthSchemes =
            "Identity.Application" + "," + IdentityServerAuthenticationDefaults.AuthenticationScheme; //Cookie + Token authentication

::

In fact as you can see ApplicationController uses both cookie and bearer token authentication scheme.

Currently ApiClient uses cookie authentication to access ApplicationController.
To see an example of external access with ApiClient and bearer authentication, you have to look at **BlazorBoilerplate.IdentityServer.Test#** projects.
