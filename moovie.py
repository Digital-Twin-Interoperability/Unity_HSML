from moviepy.editor import VideoFileClip
import os

def create_clips(input_file, output_folder, clip_duration=6):
    # Load the video file
    video = VideoFileClip(input_file)
    video_duration = video.duration

    # Calculate the number of clips to be created
    num_clips = int(video_duration // clip_duration)
    
    # Ensure output directory exists
    os.makedirs(output_folder, exist_ok=True)

    for i in range(num_clips):
        start_time = i * clip_duration
        end_time = start_time + clip_duration
        
        # Define the filename for the output clip
        output_file = os.path.join(output_folder, f"clip_{i+1}.mp4")

        # Extract the clip from the video
        clip = video.subclip(start_time, min(end_time, video_duration))
        clip.write_videofile(output_file, codec="libx264")

        print(f"Clip {i+1} saved to {output_file}")

    video.close()

def process_videos_in_folder(input_folder, output_folder_base, clip_duration=6):
    # Loop through all files in the input folder
    for filename in os.listdir(input_folder):
        if filename.endswith(".mp4"):
            input_file = os.path.join(input_folder, filename)
            output_folder = os.path.join(output_folder_base, os.path.splitext(filename)[0])

            print(f"Processing {filename}...")
            create_clips(input_file, output_folder, clip_duration)
            print(f"Finished processing {filename}\n")

# Example usage
input_folder = "digClips"        # Folder containing the input videos
output_folder_base = "droppingOutput"   # Base folder where clips will be saved
clip_duration = 6              # Duration of each clip in seconds

process_videos_in_folder(input_folder, output_folder_base, clip_duration)
