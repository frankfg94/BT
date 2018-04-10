using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;

public class AudioService : ModuleBase<ICommandContext>
{
    private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();


    IAudioClient client;
    public async Task JoinAudio(IGuild guild, IVoiceChannel channel)
    {
        try
        {   
             
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                Console.WriteLine("1");
                return;
            }
            if (channel.Guild.Id != guild.Id)
            {
                Console.WriteLine("2");
                return;
            }

            if (channel == null)
            {
                Console.WriteLine("You need to be in a voice channel, or pass one as an argument.");
                return;
            }
            
        
            IAudioClient audioClient = await channel.ConnectAsync();

            if (ConnectedChannels.TryAdd(guild.Id, audioClient))
            {
                // If you add a method to log happenings from this service,
                // you can uncomment these commented lines to make use of that.
                //Console.WriteLine("Connected to voice on {0}.", guild.Name);
                Console.WriteLine(" La connexion été effectuée sur " + guild.Name);
            }

            Console.WriteLine("\n------------------------CONNEXION CHANNEL AUDIO-----------------------------------------\n");
            PropertyInfo[] infos = audioClient.GetType().GetProperties();
            foreach(var i in infos)
            {
                Console.WriteLine(i.Name + " : " + i.GetValue(audioClient,null));
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public async Task LeaveAudio(IGuild guild)
    {
        if (ConnectedChannels.TryRemove(guild.Id, out IAudioClient client))
        {
            await client.StopAsync();
            //await Log(LogSeverity.Info, $"Disconnected from voice on {guild.Name}.");
        }
    }

    public void StopAudio()
    {
        foreach (var process in Process.GetProcessesByName("ffmpeg"))
        {
            process.Kill();
        }
    }

    public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string path)
    {
        // Your task: Get a full path to the file if the value of 'path' is only a filename.
        if (!File.Exists(path))
        {
           
            // Ne fonctionne que si libopus et libsodium sont correctement installés
            await channel.SendMessageAsync(" >> Le fichier n'existe pas à : " + path);
            return;
        }

        //var ffmpegProc = Process.GetProcessesByName("ffmpeg");
        //if (ffmpegProc.Length > 0)
        //{
        //    foreach (var p in ffmpegProc)
        //    {
        //        p.Kill();
        //    }
        //}
        
        if (ConnectedChannels.TryGetValue(guild.Id, out IAudioClient client))
        {
            Console.WriteLine("\n-----------------------------ENVOI DE L'AUDIO--------------------------------------------------------\n");
            Console.WriteLine("Client qui va être utilisé : " + client.ConnectionState);
            //HYPER MEGA IMPORTANT, UTILISER LES USING
            using (var ffmpeg = CreateStream(path))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try
                {
                    await output.CopyToAsync(discord);
                    Console.WriteLine("CopyToAsync() terminé: ");
                }
                finally { Console.WriteLine("FlushAsync() va être lancé "); await discord.FlushAsync(); }
                Console.WriteLine("Execution terminée");
                
            }
        }   
        else 
             await channel.SendMessageAsync("Merci de d'abord joindre le canal audio avec !join");
        Console.WriteLine("\n-------------------------------------------------------------------------------------\n");
    }

    async Task CanReadOrWrite( AudioOutStream discord)
    {
        await Context.Channel.SendMessageAsync(discord.CanRead.ToString());
        await Context.Channel.SendMessageAsync(discord.CanWrite.ToString());
    }

    private Process CreateStream(string path)
    {
       Console.WriteLine("Création d'un stream {" + path+ "}");
       // System.Diagnostics.Process.Start(path); 
       // Cette ligne sert à montrer que le système de path est correct
        return Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg.exe",
            Arguments = $" -i {path} -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true
        });
    }
}