namespace Mirror.FizzySteam
{
    public enum ErrorCodes : byte
    {
        None,
        IncorrectStringFormat,
        ConnectionFailed,

        /// <summary>
        /// These are all steam connection error codes to be processed.
        /// </summary>
        k_EP2PSessionErrorNone = 0,
        k_EP2PSessionErrorNotRunningApp = 1,            // target is not running the same game
        k_EP2PSessionErrorNoRightsToApp = 2,            // local user doesn't own the app that is running
        k_EP2PSessionErrorDestinationNotLoggedIn = 3,   // target user isn't connected to Steam
        k_EP2PSessionErrorTimeout = 4,                  // target isn't responding, perhaps not calling AcceptP2PSessionWithUser()
        // corporate firewalls can also block this (NAT traversal is not firewall traversal)
        // make sure that UDP ports 3478, 4379, and 4380 are open in an outbound direction
        k_EP2PSessionErrorMax = 5
    }
}
