#import "GameCenterUserVerify.h"

@implementation GameCenterUserVerify

+ (void)Verify
{
    GKLocalPlayer *localPlayer = [GKLocalPlayer localPlayer];
    if (localPlayer.authenticated)
    {
        __weak GKLocalPlayer *useLocalPlayer = localPlayer;
        [useLocalPlayer generateIdentityVerificationSignatureWithCompletionHandler: ^(NSURL * _Nullable publicKeyUrl,
                                                                                      NSData * _Nullable signature,
                                                                                      NSData * _Nullable salt,
                                                                                      uint64_t timestamp,
                                                                                      NSError * _Nullable error) {
            if (error == nil)
            {
                [self verifyPlayer: useLocalPlayer.playerID // our verify routine: below
                      publicKeyUrl: publicKeyUrl
                         signature: signature
                              salt: salt
                         timestamp: timestamp];
            }
            else
            {
                // GameCenter returned an error; deal with it here.
            UnitySendMessage("GameCenter","IOSGameGameCenterVerifyFail",[[error localizedDescription] UTF8String]);
            }
        }];
    }
    else
    {
        // User is not authenticated; it makes no sense to try to verify them.
        UnitySendMessage("GameCenter","IOSGameGameCenterVerifyFail","Game center not logined.");
    }
}

+(void)verifyPlayer: (NSString*) playerID
       publicKeyUrl: (NSURL*) publicKeyUrl
          signature: (NSData*) signature
               salt: (NSData*) salt
          timestamp: (uint64_t) timestamp
    {
        NSDictionary *paramsDict = @{ @"publicKeyUrl": [publicKeyUrl absoluteString],
                                      @"timestamp"   : [NSString stringWithFormat: @"%llu", timestamp],
                                      @"signature"   : [signature base64EncodedStringWithOptions: 0],
                                      @"salt"        : [salt base64EncodedStringWithOptions: 0],
                                      @"playerID"    : playerID,
                                      @"bundleID"    : [[NSBundle mainBundle] bundleIdentifier]
                                      };
        
        NSError *error = nil;
        NSData *bodyData = [NSJSONSerialization dataWithJSONObject: paramsDict options: 0 error: &error];
        
        if (error != nil)
        {
            NSLog(@"%s ***** dataWithJson error: %@", __PRETTY_FUNCTION__, error);
            UnitySendMessage("GameCenter","IOSGameGameCenterVerifyFail",[[error localizedDescription] UTF8String]);
        }else{
            NSString *jsonStr = [[NSString alloc] initWithData:bodyData encoding:NSUTF8StringEncoding];
            // NSLog(@"json data:%s",[jsonStr UTF8String]);
            UnitySendMessage("GameCenter","IOSGameGameCenterVerifySuccess",[jsonStr UTF8String]);
        }
    }
@end

@implementation GameCenterUserManager
extern "C"
{
    
    void CallFromUnity_GameCenterUserVerify()
    {
        NSLog(@"CallFromUnity_GameCenterUserVerify.");
        [GameCenterUserVerify Verify];
    }
    
}
@end
