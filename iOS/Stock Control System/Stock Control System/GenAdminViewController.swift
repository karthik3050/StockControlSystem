//
//  GenAdminViewController.swift
//  Stock Control System
//
//  Created by karthik reddy on 06/04/2024.
//

import UIKit

class GenAdminViewController: UIViewController {

    override func viewDidLoad() {
        super.viewDidLoad()

        // Do any additional setup after loading the view.
    }
    
    @IBAction func view(_ sender: Any) {
        let vc = self.storyboard?.instantiateViewController(withIdentifier: "GenerateViewController")
        self.present(vc!, animated: true, completion: nil)
    }
    
    
    @IBAction func viewall(_ sender: Any) {
        let vc = self.storyboard?.instantiateViewController(withIdentifier: "karu")
        self.present(vc!, animated: true, completion: nil)
    }
    
    @IBAction func write(_ sender: Any) {
        let vc = self.storyboard?.instantiateViewController(withIdentifier: "billa")
        self.present(vc!, animated: true, completion: nil)
    }
    
    @IBAction func packed(_ sender: Any) {
        let vc = self.storyboard?.instantiateViewController(withIdentifier: "pack")
        self.present(vc!, animated: true, completion: nil)
    }
    
    @IBAction func shipped(_ sender: Any) {
        let vc = self.storyboard?.instantiateViewController(withIdentifier: "ship")
        self.present(vc!, animated: true, completion: nil)
    }
    
    @IBAction func delivered(_ sender: Any) {
        let vc = self.storyboard?.instantiateViewController(withIdentifier: "deliver")
        self.present(vc!, animated: true, completion: nil)
    }
    
    
    
}
    
