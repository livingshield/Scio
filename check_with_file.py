#!/usr/bin/env python3
"""
Infrastructure check with file output
"""

import ftplib
import pymssql
import sys

output_lines = []

def log(msg):
    print(msg)
    output_lines.append(msg)

log("="*60)
log("FTP CONNECTION TEST")
log("="*60)
try:
    ftp = ftplib.FTP("windows11.aspone.cz", timeout=30)
    ftp.login("EkoBio.org_lordkikin", "Brzsilpot7!")
    log("✓ SUCCESS: Connected to FTP")
    log(f"  Current directory: {ftp.pwd()}")
    files = []
    ftp.dir(files.append)
    log(f"  Total items in root: {len(files)}")
    log("\n  First 15 items:")
    for f in files[:15]:
        log(f"    {f}")
    ftp.quit()
    ftp_ok = True
except Exception as e:
    log(f"✗ FAILED: {e}")
    ftp_ok = False

log("\n" + "="*60)
log("SQL DATABASE TEST")
log("="*60)
try:
    conn = pymssql.connect(
        server="sql8.aspone.cz",
        user="db4937",
        password="lordkikin",
        database="db4937",
        timeout=30
    )
    log("✓ SUCCESS: Connected to SQL Server")
    cursor = conn.cursor()
    
    cursor.execute("SELECT @@VERSION")
    version = cursor.fetchone()[0]
    log(f"  Version: {version.split(chr(10))[0][:100]}")
    
    cursor.execute("""
        SELECT COUNT(*) 
        FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_TYPE = 'BASE TABLE'
    """)
    table_count = cursor.fetchone()[0]
    log(f"  Total tables: {table_count}")
    
    if table_count > 0:
        cursor.execute("""
            SELECT TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE'
            ORDER BY TABLE_NAME
        """)
        tables = cursor.fetchall()
        log("\n  Tables in database:")
        for (name,) in tables[:30]:
            log(f"    - {name}")
    else:
        log("\n  Database is empty (no tables)")
    
    # Test permissions
    log("\n  Testing CREATE TABLE permission...")
    try:
        cursor.execute("CREATE TABLE _test_perm (id INT)")
        log("    ✓ Can create tables")
        cursor.execute("DROP TABLE _test_perm")
        log("    ✓ Can drop tables")
    except Exception as e:
        log(f"    ✗ Permission issue: {e}")
    
    conn.close()
    sql_ok = True
except Exception as e:
    log(f"✗ FAILED: {e}")
    sql_ok = False

log("\n" + "="*60)
log("SUMMARY")
log("="*60)
log(f"  FTP:  {'✓ OK' if ftp_ok else '✗ FAILED'}")
log(f"  SQL:  {'✓ OK' if sql_ok else '✗ FAILED'}")
log("="*60)

# Write to file
with open("infrastructure_report.txt", "w", encoding="utf-8") as f:
    f.write("\n".join(output_lines))

log("\nReport saved to: infrastructure_report.txt")
