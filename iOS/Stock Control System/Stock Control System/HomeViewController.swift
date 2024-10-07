import UIKit
import Firebase
import FirebaseDatabase

class HomeViewController: UIViewController {
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        // Do any additional setup after loading the view.
    }
    
    @IBAction func view(_ sender: Any) {
        let vc = self.storyboard?.instantiateViewController(withIdentifier: "GenerateViewController")
        self.present(vc!, animated: true, completion: nil)
    }
    
    
    @IBAction func write(_ sender: Any) {
        let vc = self.storyboard?.instantiateViewController(withIdentifier: "billa")
        self.present(vc!, animated: true, completion: nil)
    }
        
    
}
