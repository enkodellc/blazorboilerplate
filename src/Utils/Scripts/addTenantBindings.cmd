@echo Add bindings in src\.vs\BlazorBoilerplate\applicationhost.config too
@echo "<binding protocol="http" bindingInformation="*:53414:tenant1.local" />"
@echo "<binding protocol="http" bindingInformation="*:53414:tenant2.local" />"
@echo ************************************************************************
@echo Add 127.0.0.1 tenant1.local tenant2.local   to file hosts
@echo ************************************************************************
netsh http add urlacl url=http://localhost:53414/ user=everyone
pause
netsh http add urlacl url=http://tenant1.local:53414/ user=everyone
pause
netsh http add urlacl url=http://tenant2.local:53414/ user=everyone
pause