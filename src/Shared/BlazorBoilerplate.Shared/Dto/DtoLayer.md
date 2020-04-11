Data Transfer Objects (DTO). To understand this concept, we need to know that when we make calls on objects which are remote, it’s an expensive operation. 
Let’s suppose we have a fine grained object; i.e., with simple properties along with methods which manipulate those properties. 
If this object is at remote server, then calling the methods to fill these properties will lead to multiple round trips, impacting the latency. Instead of this, if we could create a DTO of that object and fill that object once on the first call, then we can save ourselves from multiple round trips. Once that DTO is obtained from remote call, we can manipulate the properties locally because now, that DTO is in our memory.

Another scenario is: Let’s suppose I have a user class for my web application. Here, user is a business object that has properties and business logic defined. We don’t need to present all the properties to the presentation layer of our application. Here, UserDTO also can help us. By doing this, our presentation layer is not tightly coupled with business objects and we can add or remove properties to DTO independently, untilthe business object is unchanged.

DTOs are also very handy for complex objects. I have read one of the articles from Shivprasad Sir where he mentioned one scenario of using the DTO - when we want to flatten one to many relationship into one. For e.g.example - suppose we have customer and address class. Then, instead of loading customer first and then iteratively loading  the addresses, we can have a CustomerAddressDTO which has all customer properties and the collection of addresses.

DTO is simple class i.e. only getter setter. There is no business logic in it.

The majority of applications are using DTOs for passing the data from one layer to another. They are light-weight and are very useful. I have just summarized the information I have got. Please have a look at the below references.

If Blazor Boilerplate in the future implements GraphQL this will alter the need for the Dto layer.

References
https://www.c-sharpcorner.com/blogs/introduction-to-data-transfer-object
http://www.codeproject.com/Articles/1050468/Data-Transfer-Object-Design-Pattern-in-Csharp
https://msdn.microsoft.com/en-us/library/ff649585.aspx