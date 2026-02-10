import ftplib

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
                log_files.append(filename)
        
        if not log_files:
            print("No log files found.")
            return
            
        # Download the latest one
        latest_log = sorted(log_files)[-1]
        print(f"Downloading {latest_log}...")
        with open("latest_server_log.txt", "wb") as f:
            ftp.retrbinary(f"RETR {latest_log}", f.write)
        
        print("Done.")
        ftp.quit()
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    main()
