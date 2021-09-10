@echo off

echo "Test 5"
echo "Test 5" > test.xls
webv -s http://localhost:4125 -f benchmark.json benchmark.json benchmark.json benchmark.json benchmark.json >> test.xls

echo "Test 4"
echo "Test 4" >> test.xls
webv -s http://localhost:4124 -f benchmark.json benchmark.json benchmark.json benchmark.json benchmark.json >> test.xls

echo "Test 3"
echo "Test 3" >> test.xls
webv -s http://localhost:4123 -f benchmark.json benchmark.json benchmark.json benchmark.json benchmark.json >> test.xls

echo "Test 2"
echo "Test 2" >> test.xls
webv -s http://localhost:4122 -f benchmark.json benchmark.json benchmark.json benchmark.json benchmark.json >> test.xls

echo "Test 1"
echo "Test 1" >> test.xls
webv -s http://localhost:4121 -f benchmark.json benchmark.json benchmark.json benchmark.json benchmark.json >> test.xls

echo "Test 0"
echo "Test 0" >> test.xls
webv -s http://localhost:4120 -f benchmark.json benchmark.json benchmark.json benchmark.json benchmark.json >> test.xls
