using System.Threading.Tasks;
using Discord.Commands;
using Discord.Audio;
using Discord;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public class AudioModule : ModuleBase<ICommandContext>
{
    // Scroll down further for the AudioService.
    // Like, way down
    private readonly AudioService _service;

    // Remember to add an instance of the AudioService
    // to your IServiceCollection when you initialize your bot
    public AudioModule(AudioService service)
    {
        _service = service;
    }
    

    // You *MUST* mark these commands with 'RunMode.Async'
    // otherwise the bot will not respond until the Task times out.
    [Command("join", RunMode = RunMode.Async)]
    public async Task JoinCmd()
    {
        await ReplyAsync("J'ai détecté votre commande, je ne peux pas vous dire si l'audio fonctionne pour le moment");
        await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
    }


    List<IGuildUser> list = new List<IGuildUser>();


    private async Task SpeakDetected(ulong id, bool updated)
    {
        if (updated) await ReplyAsync("Quelqu'un parle");
    }



    // Remember to add preconditions to your commands,
    // this is merely the minimal amount necessary.
    // Adding more commands of your own is also encouraged.
    [Command("leave", RunMode = RunMode.Async)]
    public async Task LeaveCmd()
    {
        await _service.LeaveAudio(Context.Guild);
    }

    [Command("play", RunMode = RunMode.Async)]
    public async Task PlayCmd([Remainder] string song)
    {
        await ReplyAsync("Vous voulez jouer de l'audio! c'est parti :smiley: ");
        await _service.SendAudioAsync(Context.Guild, Context.Channel, song);
    }
    /*
    [Command("kill", RunMode = RunMode.Async)]
        public async void Kill([Remainder] string name)
        {
            var users = Context.Guild;
            for(int i = 0; i < users. ; i++)
            {
                
                if (u.Username.ToUpper() == name.ToUpper() || u.Nickname.ToUpper() == name.ToUpper())
                {
                    await u.ModifyAsync(x => 
                    .Nickname = "[MORT]" + u.Username);
                    await ReplyAsync(":scream: " + u.Username + " vient de mourir!");
                    await _service.SendAudioAsync(Context.Guild,Context.Channel,@"C:/Users/gillioen/Desktop/death.wav");
                    
                }
            }

        }
        */

    
}