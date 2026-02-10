import ftplib

# FTP Settings
FTP_SERVER = "windows11.aspone.cz"
FTP_USERNAME = "EkoBio.org_lordkikin"
FTP_PASSWORD = "Brzsilpot7!"
REMOTE_ROOT = "/www/scio"

def main():
    try:
        ftp = ftplib.FTP(FTP_SERVER)
        ftp.login(FTP_USERNAME, FTP_PASSWORD)
        ftp.set_pasv(True)
        ftp.cwd(REMOTE_ROOT)
        
        print("Attempting to rename ScioApp.exe...")
        try:
            ftp.rename("ScioApp.exe", "ScioApp.exe.test")
            print("RENAME SUCCESSFUL - File is NOT locked.")
            ftp.rename("ScioApp.exe.test", "ScioApp.exe")
        except Exception as e:
            print(f"RENAME FAILED - File is likely LOCKED: {e}")
            
        print("\nChecking file sizes:")
        files = []
        ftp.retrlines('LIST', files.append)
        for f in files:
            if "ScioApp.exe" in f or "ScioApp.dll" in f or "web.config" in f:
                print(f)
                
        ftp.quit()
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    main()
