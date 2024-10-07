//
//  ViewController.swift
//  Stock Control System
//
//  Created by karthik reddy on 26/03/2024.
//

import UIKit
import Firebase
import FirebaseDatabase
import CommonCrypto

class ViewController: UIViewController {

    // Outlets
    @IBOutlet weak var uname: UITextField!
    @IBOutlet weak var pname: UITextField!

    // Actions
    @IBAction func login(_ sender: Any) {
        guard let username = uname.text, let password = pname.text, !username.isEmpty, !password.isEmpty else {
            showAlert(title: "Error", message: "Username and password cannot be empty")
            return
        }

        if username == "admin" && password == "admin" {
            navigateToThirdViewController()
        } else {
            retrieveUserDataAndLogin(username: username, password: password)
        }
    }

    @IBAction func register(_ sender: Any) {
        let vc = self.storyboard?.instantiateViewController(withIdentifier: "reg")
        self.present(vc!, animated: true, completion: nil)
    }

    // Lifecycle
    override func viewDidLoad() {
        super.viewDidLoad()
        // Do any additional setup after loading the view.
        pname.isSecureTextEntry = true

    }

    override func touchesBegan(_ touches: Set<UITouch>, with event: UIEvent?) {
        uname.resignFirstResponder()
        pname.resignFirstResponder()
    }

    // Helper Methods
    private func retrieveUserDataAndLogin(username: String, password: String) {
        let databaseReference = Database.database(url: "https://stockcontrol-f2ff7-default-rtdb.firebaseio.com").reference()

        databaseReference.child("Users").child(username).observeSingleEvent(of: .value) { (snapshot) in
            if let userData = snapshot.value as? [String: Any], let storedPassword = userData["password"] as? String {
                if self.verifyPassword(password, hashedPassword: storedPassword) {
                    print("Login successful!")
                    self.navigateToFourthViewController()
                } else {
                    self.showAlert(title: "Error", message: "Invalid username or password")
                }
            } else {
                self.showAlert(title: "Error", message: "Invalid username or password")
            }
        } withCancel: { (error) in
            self.showAlert(title: "Error", message: "An error occurred: \(error.localizedDescription)")
        }
    }
    
    // SHA-256 Algorithm sourced from CryptoSwift Library and NIST
    private func verifyPassword(_ password: String, hashedPassword: String) -> Bool {
        guard let hashedData = hashedPassword.data(using: .utf8) else {
            return false
        }
        // Hash the input password
        var hash = [UInt8](repeating: 0, count: Int(CC_SHA256_DIGEST_LENGTH))
        password.data(using: .utf8)?.withUnsafeBytes {
            _ = CC_SHA256($0.baseAddress, CC_LONG(password.utf8.count), &hash)
        }
        let hashedString = hash.map { String(format: "%02x", $0) }.joined()

        return hashedString == hashedPassword
    }

    
    
    private func showAlert(title: String, message: String) {
        let alertController = UIAlertController(title: title, message: message, preferredStyle: .alert)
        alertController.addAction(UIAlertAction(title: "OK", style: .default, handler: nil))
        present(alertController, animated: true, completion: nil)
    }

    // Navigation
    private func navigateToFourthViewController() {
        let vc = self.storyboard?.instantiateViewController(withIdentifier: "Home")
        self.present(vc!, animated: true, completion: nil)
    }

    private func navigateToThirdViewController() {
        let vc = self.storyboard?.instantiateViewController(withIdentifier: "genadmin")
        self.present(vc!, animated: true, completion: nil)
    }
}
