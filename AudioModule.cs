using System.Threading.Tasks;
using Discord.Commands;
using Discord.Audio;
using Discord;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using BT;
using System;
using Discord.WebSocket;

public class AudioModule : ModuleBase<ICommandContext>
{
    // Scroll down further for the AudioService.
    // Like, way down
    private readonly AudioService _service;
    private SocketCommandContext commandContext ;
    // Remember to add an instance of the AudioService
    // to your IServiceCollection when you initialize your botx
    public AudioModule(AudioService service, SocketCommandContext cm = null)
    {
        commandContext = (SocketCommandContext)service.Context;
        _service = service;
        if(cm != null)   commandContext = cm;
        Console.WriteLine("Audio Module crée");
    }

    public async Task Test()
    {
       await Console.Out.WriteLineAsync("Test OK Console");
        try { await commandContext.Channel.SendMessageAsync("Test OK Discord"); }
        catch { await Console.Out.WriteLineAsync("ReplyAsync problème de référence"); }
        await Music1();
    }

    // You *MUST* mark these commands with 'RunMode.Async'
    // otherwise the bot will not respond until the Task times out.
    public async Task JoinCmd()
    {
        if(commandContext == null)
        {
           // await Context.Channel.SendMessageAsync("J'ai détecté votre commande, je ne peux pas vous dire si l'audio fonctionne pour le moment");
            await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        }
        else
        {
           // await commandContext.Channel.SendMessageAsync("J'ai détecté votre commande, je ne peux pas vous dire si l'audio fonctionne pour le moment");
            await _service.JoinAudio(commandContext.Guild, (commandContext.User as IVoiceState).VoiceChannel);
        }
    }

    public void StopCmd()
    {
        _service.StopAudio();
    }


    [Command("music1",RunMode = RunMode.Async)]
    public async Task Music1()  
    {
        try
        {
            //await ReplyAsync("Lancement fonction");
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
        }
        await JoinCmd();
        await PlayCmd(song: @"D:/Téléchargements/koh.mp3");
    }

    [Command("music2", RunMode = RunMode.Async)]
    public async Task Music2()
    {
        await JoinCmd();
        await PlayCmd(song: "r6.mp3");
    }

    [Command("doorCloseSFX", RunMode = RunMode.Async)]
    public async Task DoorCloseSFX()
    {
        await JoinCmd();
        await PlayCmd(song: "roomClose.mp3");
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
        if(commandContext == null)
        {
          //  await Context.Channel.SendMessageAsync("Vous voulez jouer de l'audio! c'est parti :smiley: ");
            await _service.SendAudioAsync(Context.Guild, Context.Channel, song);
        }
        else
        {
           // await commandContext.Channel.SendMessageAsync("Vous voulez jouer de l'audio! c'est parti :smiley: ");
            await _service.SendAudioAsync(commandContext.Guild, commandContext.Channel, song);
        }
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