# SSHScripter
Windows utility that will let you automate tasks on multiple servers. The inspiration comes from times where you need to let a member of you IT go that may have access to all your servers root accounts. This will allow you to change the passwords on each server, MySQL, and any other service that can be done from the command line.

You can have multiple groups of servers, which would each have their own command scripts. The scripts are organized into "Groups" rather than per server, that way if you have 20 servers with the same configuration, you can put them all in the same group and it will execute the same set of instructions for each server in the group.

Features are being added as I have time. Improvements should include scanning for saved SSH key logins, adding pauses in commands, allowing you to edit the "Response" from the server and selecting whether it needs to be an exact match or just contain the phrase within the response.

Default license is GNU LGPLv3, if you want to commercialize it, let's talk.
