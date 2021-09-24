MultiTenancy
============

The implementation of multitenancy is based on `Finbuckle.MultiTenant <https://www.finbuckle.com/MultiTenant>`_.

The host strategy is used:

::

       services.AddMultiTenant()
               .WithHostStrategy("__tenant__")
               .WithEFCoreStore<TenantStoreDbContext>()
               .WithFallbackStrategy(Settings.DefaultTenantId);

Setup Visual Studio and Windows for multiple bindings
-----------------------------------------------------

Open \\src\\.vs\\BlazorBoilerplate\config\\\ **applicationhost.config** and
add these bindings:

::

       <site name="BlazorBoilerplate.Server" id="...">
         <application path="/" applicationPool="BlazorBoilerplate.Server AppPool">
           <virtualDirectory path="/" physicalPath="...\src\Server\BlazorBoilerplate.Server" />
         </application>
         <bindings>
           <binding protocol="http" bindingInformation="*:53414:localhost" />
           <binding protocol="http" bindingInformation="*:53414:tenant1.local" />
           <binding protocol="http" bindingInformation="*:53414:tenant2.local" />
         </bindings>
       </site>

Run as administrator \\src\\Utils\\Scripts\\\ **addTenantBindings.cmd** to
enable access in Windows to the above bindings. It contains commands
like

::

       netsh http add urlacl url=http://tenant1.local:53414/ user=everyone

Open as administrator C:\\Windows\\System32\\drivers\\etc\\\ **hosts** and
add the following line:

::

       127.0.0.1           tenant1.local tenant2.local

Delete the previous database if any, because DatabaseInitializer inits
TenantInfo for the two tenants.

Run debug and in Admin UI you will find a MultiTenancy section with
links to the two demo tenants.

In the following screenshot you see the configuration with the two online demo tenants.

.. image:: /images/admin-multitenancy.png
   :align: center
