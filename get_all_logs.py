import ftplib
import os

# FTP Settings
FTP_SERVER = "windows11.aspone.cz"
FTP_USERNAME = "EkoBio.org_lordkikin"
FTP_PASSWORD = "Brzsilpot7!"
REMOTE_LOG_DIR = "/www/scio/logs"

def main():
    try:
        ftp = ftplib.FTP(FTP_SERVER)
        ftp.login(FTP_USERNAME, FTP_PASSWORD)
        ftp.set_pasv(True)
        
        ftp.cwd(REMOTE_LOG_DIR)
        files = []
        ftp.retrlines('LIST', files.append)
        
        log_files = []
        for line in files:
            parts = line.split()
            if not parts: continue
            filename = " ".join(parts[8:])
            if filename.startswith("stdout"):
                # Format: -rw-r--r-- 1 user group size Date Time filename
                size = int(parts[4])
                log_files.append((filename, size))
        
        if not log_files:
            print("No log files found.")
            return
            
        # Download all log files with their sizes
        for filename, size in log_files:
            print(f"Downloading {filename} ({size} bytes)...")
            try:
                with open(filename, "wb") as f:
                    ftp.retrbinary(f"RETR {filename}", f.write)
            except Exception as e:
                print(f"  Failed (probably locked): {e}")
        
        ftp.quit()
        print("Done.")
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    main()
