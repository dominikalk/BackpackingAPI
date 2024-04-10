

# A Network-Based Backpacking API

_ASP.NET Core 8 | Microsoft Identity | Entity Framework Core | SignalR | MSTest | Google API | PositionStack API_

This ASP.NET Core 8 API was developed for my Computer Science BSc Final Year Individual Project. 

## Introduction
The nature of backpacking is a volatile one. Backpackers are often changing locations, changing plans, and meeting new people (Forgeard, 2022). Meeting new people is a crucial aspect of backpacking as it can open your mind to new cultures and lead to new and exciting adventures. But often this fun must come to an end when traveling to new locations, and those friends made along the way will never be seen again.

Currently, backpackers find it difficult to stay in touch with their peers and re-meet them in the future. Current methods include staying in touch with specific friends over regular social media platforms, though this only provides a primitive method to do so and does not upscale well when you simply want to meet any of the friends made in the past. 

Researching blog and article sites on how to stay in touch with former backpackers, you are directed to a plethora of articles on “how to make friends backpacking” (Davies, 2024) but very few even attempt to address pre-existing friends. Those that do mention this issue only suggest solutions such as the previously mentioned primitive methods (Kendle, 2011). Although some platforms do exist such as Backpackr (Backpackr, 2022) and WanderMate (WanderMate, 2023), they aim to address the issue from the perspective of finding friends and not maintaining them. Additionally, concerns have been raised on online forums regarding the safety of those platforms and whether there are “creepy people on there” (terrtravels, 2016)

This API aims to address this issue by giving backpackers a means to keep track of old friends they met in their travels, with the final goal of providing a means to meet again. Users can create accounts, log current and future locations, network with other users, access relevant information, and chat with other users.

## The API

This API makes use of Microsoft's ASP.NET Core 8 framework. 

For authentication, the API makes use of Microsoft's Identity platform. It uses bearer token authentication, and due to the fact that the authentication endpoint logic differs from the default authentication endpoints they have been implemented manually. 

Entity Framework Core is used for object-relational mapping to the postgres database the API utilises. Additionally, for testing, the solution makes use of MSTest along with Moq.

### User

The functionality related to the user and their account follows the pattern of most applications and includes most of the functionality provided by them. This API does require that the user's email is confirmed in order for them to be able to log in and access authenticated endpoints.

### Location

The user has the ability to log their visited and planned locations. Both location types follow separate rules and serve different purposes. 

Visited locations are locations the user has visited in the past or is at currently. The only way to log a visited location is to log a current location. Current locations are simply visited locations that haven't been departed yet. When a new location is logged, the previous one is departed by updating the `departDate` , and the new one is added. When updating visited locations, it must follow the rule of the user not being able to have been at 2 visited locations at once.

Planned locations are more informal and simply denote the user's intention for the future. Multiple planned locations can be logged at the same time for this reason. 

### Geocoding

Forward geocoding is the process of obtaining coordinates based on an address or search string. In this API, this is used for obtaining the coordinates and name to associate with a planned location the user is logging. This API makes use of PositionStack's Geocoding API to return a list of locations based on the user's search string. E.g. searching for "London" gives the following results:
```
[
	{
		"label":  "London, England, United Kingdom",
		"longitude":  -0.099076,
		"latitude":  51.509647,
		"region":  "Greater London"
	},
	{
		"label":  "London, ON, Canada",
		"longitude":  -81.23518,
		"latitude":  42.959034,
		"region":  "Ontario"
	}
]
```
Reverse geocoding is the process of obtaining location information based on coordinates provided. In this API, this is used for obtaining the name of the location the user is currently at when logging their current location. This API makes use of Google's Geocoding API to return a list of possible location information based on the coordinates provided. E.g. providing `longitude=-0.122106&latitude=51.499265` gives the following results:
```
[
	"London Borough of Lambeth, London, UK",
	"London, UK",
	"Greater London, UK",
	"United Kingdom"
]
```

### Network

The user is also able to network with other users on the system. The user is able to fetch their friend requests and accept or reject them. They are also able to search for other users to send friend requests to them. 

If a user no longer wants to be friends with someone, they can unfriend them. Additionally, if the user wants to block another user, they can do so too, which means that the blocked user will not be visible in any results and they will not be visible to the blocked user either. It is also possible to access a list of your blocked users (the only response that doesn't filter them out) to unblock them. 

### Friend

Once two users are friends, they can access more private information about each other such as their current location, planned future locations, or past visited locations. This allows for the user to plan their own future travels so that they could meet with their friends. 

### Chat

If two users are friends, they can also contact each other using this API to message to meet up. The chat functionality works in a similar way to any other chat system with basic features such as creating a chat and sending messages. 

Additionally, when creating a chat or sending a message, SignalR is used for live updates so that the other user in the chat has their client side application update instantly without the need for a page refresh. 

## The Database Structure

![BackpackingDB](https://github.com/dominikalk/BackpackingAPI/assets/55254805/51bda08d-23aa-4539-a2d9-9a5ee6f4e3c7)

## The Endpoints

For further documentation on the endpoints such as input params and response types, clone and run the API and view the Swagger API documentation.

### User

Endpoints related to the user's account information and user authentication. The user is required to have an email that has been confirmed to be able to log in. Once the email has been confirmed, they will be able to log in and access authenticated endpoints. 

**GET** `/v1/user/available` returns whether the provided username is available for new account creation

**POST** `/v1/user/register` registers a new user

**POST** `/v1/user/login` logs in user and returns bearer authentication token

**POST** `/v1/user/refresh` refreshes a user's session

**GET** `/v1/user/confirmEmail` confirms a user's email 

**POST** `/v1/user/resendConfirmationEmail` resends a confirmation email to the user

**POST** `/v1/user/forgotPassword` sends an email allowing the user to reset their password

**POST** `/v1/user/resetPassword` resets the user's password

**GET** `/v1/user/profile` returns the user's profile information

**PUT** `/v1/user/profile` updates the user's profile information

**POST** `/v1/user/email` sends a confirmation email to the user to reset the email

### Location

Endpoints related to the logged in user's locations. Using these, they are able to log visited locations (including current location), as well as planned future locations. 

**GET** `/v1/location/current` returns the user's logged current location

**POST** `/v1/location/current` logs the information provided as the user's current location

**PATCH** `/v1/location/current/depart` updates the current location to be departed

**PUT** `/v1/location/visited/{location_id}` updates the past visited location of the id provided

**GET** `/v1/location/visited` returns a paginated list of the past visited locations

**POST** `/v1/location/planned` logs a future planned location with the information provided

**PUT** `/v1/location/planned/{location_id}` updates a future planned location

**GET** `/v1/location/planned` returns a paginated list of the future planned locations

**GET** `/v1/location/{location_id}` returns the location of the id provided

**DELETE** `/v1/location/{location_id}` deletes the location of the id provided

### Geocoding

Endpoints related to geocoding. Given a search string, or coordinates, the user should be able to select their intended location information to be used for logging a visited or planned location.

**GET** `/v1/geocoding/forward` returns possible locations from a query string

**GET** `/v1/geocoding/reverse` returns possible locations from coordinates

### Network

Endpoints related to networking with other users. This includes getting, sending, accepting, and rejecting friend requests, as well as blocking users. Additionally it includes endpoints related to getting paginated lists of users and friends.

**GET** `/v1/network/{user_id}` returns profile of the user with the id

**GET** `/v1/network/search` returns a paginated list of users from a query string

**DELETE** `/v1/network/unfriend/{user_id}` unfriends user with the id

**GET** `/v1/network/friends` return paginated list of friends

**POST** `/v1/network/request/{user_id}` sends a friend request to the user with the id

**PATCH** `/v1/network/request/accept/{request_id}` accepts friend request with the id

**DELETE** `/v1/network/request/reject/{request_id}` rejects friend request with the id

**GET** `/v1/network/requests` returns a paginated list of friend requests

**POST** `/v1/network/block/{user_id}` blocks user with the id

**DELETE** `/v1/network/unblock/{user_id}` unblocks user with the id

**GET** `/v1/network/unblock/{user_id}` returns paginated list of blocked users

### Friend

Endpoints related to fetching locations related to the user's friends allowing the user to plan potential meet ups with friends who will be in similar locations to them.

**GET** `/v1/friend/current` returns a paginated list of the user's friend's current locations

**GET** `/v1/friend/{friend_id}/visited` returns a paginated list of the friend with the id's visited locations

**GET** `/v1/friend/{friend_id}/planned` returns a paginated list of the friend with the id's planned locations

### Chat

Endpoints related to the user's chats with other user's. This allows them to communicate with other users and plan potential future meet ups. Additionally, the `CreateChat` and `CreateChatMessage` endpoints send a live update via SignalR to the users that are part of the chat.

**GET** `/v1/chat` returns a paginated list of the user's chats

**GET** `/v1/chat/{chat_id}` returns the chat with the id

**GET** `/v1/chat/{chat_id}/messages` returns a paginated list of the messages part of the chat with the id

**POST** `/v1/chat/private` creates a private chat

**POST** `/v1/chat/{chat_id}/message` adds a message to the chat with the id

**PATCH** `/v1/chat/{chat_id}/read` marks the chat with the id as read

**GET** `/v1/chat/unread` returns the number of unread messages in all the user's chats

## References

Backpackr. 2022. Available at: https://backpackr.org/ [Accessed 4 February 2024]

Davies, W. 2024. 10 Surefire Ways to Make Friends While Traveling Solo. Go Overseas 4 January. Available at: https://www.gooverseas.com/blog/how-to-make-friends-traveling-solo [Accessed: 28 January 2024]

Forgeard, V. 2022. What is Backpacking – And Why You Should Try It. Brilliantio 7 December. Available at: https://brilliantio.com/what-is-backpacking/ [Accessed 4 February 2024]

Kendle, A. 2011. Why and How to Stay in Touch With Your Travel Friends. Vagabondish 20 September. Available at: https://vagabondish.com/why-how-stay-in-touch-with-travel-friends/ [Accessed: 28 January 2024]

terrtravels. 2016. Re: Has anyone tried travel social apps like Backpackr or Wandermates? Are they good? [Online comment]. Available at: https://www.reddit.com/r/solotravel/comments/4o2gkj/has_anyone_tried_travel_social_ap ps_like/ [Accessed 28 January 2024]

WanderMate. 2023. Available at: https://wandermate.co.uk/ [Accessed 4 February 2024]
