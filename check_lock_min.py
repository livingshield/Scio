import ftplib
FTP_SERVER = "windows11.aspone.cz"
FTP_USERNAME = "EkoBio.org_lordkikin"
FTP_PASSWORD = "Brzsilpot7!"
def main():
    ftp = ftplib.FTP(FTP_SERVER)
    ftp.login(FTP_USERNAME, FTP_PASSWORD)
    ftp.cwd("/www/scio")
    try:
        ftp.rename("ScioApp.exe", "ScioApp.exe.test")
        print("OK_UNLOCKED")
        ftp.rename("ScioApp.exe.test", "ScioApp.exe")
    except:
        print("LOCKED")
    ftp.quit()
if __name__ == "__main__":
    main()
